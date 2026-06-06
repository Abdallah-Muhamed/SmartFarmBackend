using Microsoft.EntityFrameworkCore;
using Smart_Farm.Application.Abstractions;
using Smart_Farm.Common;
using Smart_Farm.DTOS;
using Smart_Farm.Models;

namespace Smart_Farm.Application.Services;

/// <summary>
/// Daily soil-water balance + irrigation tracking.
/// Units:
///   - ET0, ETc, EffRain, Irrig, Depletion, TAW, RAW: millimetres (mm)
///   - 1 mm over 1 feddan = 4.2 m³  (1 feddan = 4200 m²)
/// </summary>
public class WaterBalanceService : IWaterBalanceService
{
    private readonly farContext _db;
    private readonly IWeatherProvider _weather;
    private readonly IIrrigationAdviceGenerator _adviceGenerator;
    private readonly ILogger<WaterBalanceService> _logger;

    private const double MM_TO_M3_PER_FEDDAN = IrrigationMath.MmToM3PerFeddan;

    public WaterBalanceService(
        farContext db,
        IWeatherProvider weather,
        IIrrigationAdviceGenerator adviceGenerator,
        ILogger<WaterBalanceService> logger)
    {
        _db = db;
        _weather = weather;
        _adviceGenerator = adviceGenerator;
        _logger = logger;
    }

    public Task<IrrigationDayDto> GetDayAsync(int cid, DateOnly date, bool includeAdvice, CancellationToken cancellationToken) =>
        GetDayInternalAsync(cid, date, includeAdvice, cancellationToken);

    public Task<IrrigationDayDto> SyncDayAsync(int cid, DateOnly date, CancellationToken cancellationToken) =>
        GetDayInternalAsync(cid, date, includeAdvice: false, cancellationToken);

    public async Task<IReadOnlyList<IrrigationCalendarDayDto>> GetCalendarAsync(
        int cid, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        if (to < from)
            throw new InvalidOperationException("'to' must be on or after 'from'.");

        var crop = await LoadCropAsync(cid, cancellationToken);
        if (crop.Start_date is null)
            throw new InvalidOperationException($"Crop {cid} has no start date.");

        if (from < crop.Start_date.Value)
            from = crop.Start_date.Value;

        await SyncThroughAsync(crop, to, cancellationToken);

        var logs = await _db.CROP_WATER_BALANCE_LOGs
            .AsNoTracking()
            .Where(x => x.Cid == cid && x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        var area = crop.Area_size ?? 1m;
        return logs.Select(log => MapCalendarDay(log, area)).ToList();
    }

    public async Task<IrrigationDayDto> RecordAsync(
        int cid, DateOnly date, bool applied, decimal? appliedLiters, CancellationToken cancellationToken)
    {
        var crop = await LoadCropAsync(cid, cancellationToken);
        ValidateDate(crop, date);
        await SyncThroughAsync(crop, date, cancellationToken);

        var log = await _db.CROP_WATER_BALANCE_LOGs
            .FirstOrDefaultAsync(x => x.Cid == cid && x.Date == date, cancellationToken)
            ?? throw new InvalidOperationException($"No balance log for crop {cid} on {date:yyyy-MM-dd}.");

        var areaFeddan = crop.Area_size ?? 1m;
        log.WasApplied = applied;
        log.AppliedAt = DateTime.UtcNow;

        if (applied)
        {
            var appliedMm = appliedLiters.HasValue
                ? IrrigationMath.LitersToMm(appliedLiters.Value, areaFeddan)
                : log.Irrig_mm ?? 0m;

            log.Applied_mm = appliedMm;
            log.DeplEnd_mm = IrrigationMath.DepletionEndAfterApplication(
                log.DeplAfterEt_mm ?? log.DeplStart_mm ?? 0m,
                appliedMm,
                log.Irrig_mm ?? 0m);
        }
        else
        {
            log.Applied_mm = 0m;
            log.DeplEnd_mm = log.DeplAfterEt_mm ?? log.DeplStart_mm ?? 0m;
        }

        crop.Depletion_mm = log.DeplEnd_mm;
        crop.LastBalanceDate = date;

        await _db.SaveChangesAsync(cancellationToken);

        var today = FarmTime.EgyptToday();
        if (date < today)
            await RecomputeForwardAsync(crop, date.AddDays(1), today, cancellationToken);

        crop = await LoadCropAsync(cid, cancellationToken);
        log = await _db.CROP_WATER_BALANCE_LOGs
            .AsNoTracking()
            .FirstAsync(x => x.Cid == cid && x.Date == date, cancellationToken);

        return MapDayDto(crop, log);
    }

    private async Task<IrrigationDayDto> GetDayInternalAsync(
        int cid, DateOnly date, bool includeAdvice, CancellationToken cancellationToken)
    {
        var crop = await LoadCropAsync(cid, cancellationToken);
        ValidateDate(crop, date);
        await SyncThroughAsync(crop, date, cancellationToken);

        var log = await _db.CROP_WATER_BALANCE_LOGs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Cid == cid && x.Date == date, cancellationToken)
            ?? throw new InvalidOperationException($"No balance log for crop {cid} on {date:yyyy-MM-dd}.");

        var dto = MapDayDto(crop, log);

        if (includeAdvice)
        {
            try
            {
                var lat = ResolveLatitude(crop);
                var lon = ResolveLongitude(crop);
                var wx = await _weather.GetDailyAsync(lat, lon, date, cancellationToken);
                dto.Reasoning = await _adviceGenerator.GenerateArabicAdviceAsync(
                    new IrrigationAdviceInput
                    {
                        Recommendation = ToLegacyRecommendation(dto),
                        Rain_mm = wx.Rain_mm,
                        Tmin_C = wx.Tmin_C,
                        Tmax_C = wx.Tmax_C
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Irrigation advice failed for crop {Cid} on {Date}", cid, date);
                dto.Reasoning = CreatePlaceholderAdvice(dto);
            }
        }

        return dto;
    }

    private async System.Threading.Tasks.Task SyncThroughAsync(CROP crop, DateOnly throughDate, CancellationToken cancellationToken)
    {
        if (crop.Start_date is null)
            throw new InvalidOperationException($"Crop {crop.Cid} has no start date.");

        if (throughDate < crop.Start_date.Value)
            return;

        var startDate = crop.LastBalanceDate?.AddDays(1) ?? crop.Start_date.Value;
        if (startDate < crop.Start_date.Value)
            startDate = crop.Start_date.Value;

        if (startDate > throughDate)
            return;

        double deplStart = await ResolveDepletionAtStartAsync(crop, startDate, cancellationToken);

        for (var day = startDate; day <= throughDate; day = day.AddDays(1))
        {
            deplStart = await ComputeAndPersistDayAsync(crop, day, deplStart, cancellationToken);
        }
    }

    private async Task<double> ResolveDepletionAtStartAsync(
        CROP crop, DateOnly startDate, CancellationToken cancellationToken)
    {
        var prior = startDate.AddDays(-1);
        var priorLog = await _db.CROP_WATER_BALANCE_LOGs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Cid == crop.Cid && x.Date == prior, cancellationToken);

        if (priorLog?.DeplEnd_mm is not null)
            return (double)priorLog.DeplEnd_mm.Value;

        if (crop.LastBalanceDate == prior && crop.Depletion_mm is not null)
            return (double)crop.Depletion_mm.Value;

        return (double?)crop.Depletion_mm ?? 0.0;
    }

    private async Task<double> ComputeAndPersistDayAsync(
        CROP crop, DateOnly date, double deplStart, CancellationToken cancellationToken)
    {
        var computation = await BuildDayComputationAsync(crop, date, deplStart, cancellationToken);

        var existing = await _db.CROP_WATER_BALANCE_LOGs
            .FirstOrDefaultAsync(x => x.Cid == crop.Cid && x.Date == date, cancellationToken);

        if (existing is null)
        {
            existing = new CROP_WATER_BALANCE_LOG { Cid = crop.Cid, Date = date };
            _db.CROP_WATER_BALANCE_LOGs.Add(existing);
        }

        double deplEnd;
        if (existing.WasApplied == true)
        {
            deplEnd = (double)(existing.DeplEnd_mm ?? (decimal)computation.DeplAfterEt);
        }
        else
        {
            deplEnd = computation.DeplAfterEt;
            existing.DeplEnd_mm = Round2(deplEnd);
        }

        existing.ET0_mm = Round2(computation.Et0);
        existing.Kc = Round2(computation.Kc);
        existing.ETc_mm = Round2(computation.Etc);
        existing.EffRain_mm = Round2(computation.EffRain);
        existing.Irrig_mm = Round2(computation.RecommendedIrrigMm);
        existing.IsIrrigationDay = computation.IsIrrigationDay;
        existing.DeplStart_mm = Round2(deplStart);
        existing.DeplAfterEt_mm = Round2(computation.DeplAfterEt);
        existing.TAW_mm = Round2(computation.TawMm);
        existing.RAW_mm = Round2(computation.RawMm);
        existing.Recommended_Liters = computation.RecommendedLiters;
        existing.StageName = computation.StageName;
        existing.Note = computation.SoilNote;

        crop.Depletion_mm = (decimal)Math.Round(deplEnd, 2);
        crop.LastBalanceDate = date;

        await _db.SaveChangesAsync(cancellationToken);
        return deplEnd;
    }

    private async System.Threading.Tasks.Task RecomputeForwardAsync(
        CROP crop, DateOnly fromDate, DateOnly throughDate, CancellationToken cancellationToken)
    {
        if (fromDate > throughDate)
            return;

        crop = await LoadCropAsync(crop.Cid, cancellationToken);
        var deplStart = await ResolveDepletionAtStartAsync(crop, fromDate, cancellationToken);

        for (var day = fromDate; day <= throughDate; day = day.AddDays(1))
        {
            deplStart = await ComputeAndPersistDayAsync(crop, day, deplStart, cancellationToken);
        }
    }

    private async Task<DayComputation> BuildDayComputationAsync(
        CROP crop, DateOnly date, double deplStart, CancellationToken cancellationToken)
    {
        if (crop.Pid is null)
            throw new InvalidOperationException($"Crop {crop.Cid} has no plant linked.");

        var stages = await _db.PLANT_STAGEs
            .Where(s => s.Pid == crop.Pid)
            .OrderBy(s => s.Stage_order)
            .ToListAsync(cancellationToken);

        if (stages.Count == 0)
            throw new InvalidOperationException($"No PLANT_STAGE rows for Pid={crop.Pid}.");

        var daysSinceStart = Math.Max(0, date.DayNumber - crop.Start_date!.Value.DayNumber);
        var currentStage = IrrigationMath.ResolveCurrentStage(stages, daysSinceStart);
        var stageName = currentStage.Name_stage ?? $"مرحلة {currentStage.Stage_order}";

        var template = await _db.PLANT_IRRIGATION_TEMPLATEs
            .FirstOrDefaultAsync(t => t.PSid == currentStage.PSid, cancellationToken);

        var soilType = crop.Soil_type
                       ?? crop.FarmNavigation?.Default_Soil_type
                       ?? "loam";
        var areaFeddan = crop.Area_size ?? 1m;

        var (tawPerMetre, soilNote) = SoilLookup.GetTawPerMetre(soilType);
        double kc = (double?)template?.Kc ?? 1.00;
        double p = (double?)template?.p_fraction ?? 0.50;
        double zr = (double?)template?.Zr_m ?? 0.40;
        double tawMm = tawPerMetre * zr;
        double rawMm = p * tawMm;

        var lat = ResolveLatitude(crop);
        var lon = ResolveLongitude(crop);
        var wx = await _weather.GetDailyAsync(lat, lon, date, cancellationToken);
        double et0 = Hargreaves.Compute(wx.Tmin_C, wx.Tmax_C, lat, date);
        double etc = kc * et0;

        double effRain = IrrigationMath.EffectiveRain(wx.Rain_mm, deplStart, tawMm);
        double deplAfterEt = IrrigationMath.DepletionAfterEt(deplStart, etc, effRain);
        bool isIrrigDay = IrrigationMath.IsIrrigationDay(deplAfterEt, rawMm);
        double recommendedIrrigMm = IrrigationMath.RecommendedIrrigationMm(deplAfterEt, isIrrigDay);
        double deplEnd = deplAfterEt;

        decimal m3PerFeddan = (decimal)Math.Round(recommendedIrrigMm * MM_TO_M3_PER_FEDDAN, 2);
        decimal liters = Math.Round(m3PerFeddan * areaFeddan * 1000m, 0);

        return new DayComputation(
            Et0: et0,
            Kc: kc,
            Etc: etc,
            EffRain: effRain,
            TawMm: tawMm,
            RawMm: rawMm,
            DeplAfterEt: deplAfterEt,
            DeplEnd: deplEnd,
            IsIrrigationDay: isIrrigDay,
            RecommendedIrrigMm: recommendedIrrigMm,
            RecommendedLiters: liters,
            StageName: stageName,
            SoilNote: soilNote,
            AreaFeddan: areaFeddan);
    }

    private async Task<CROP> LoadCropAsync(int cid, CancellationToken cancellationToken) =>
        await _db.CROPs
            .Include(c => c.PidNavigation)
            .Include(c => c.UidNavigation)
            .Include(c => c.FarmNavigation)
            .FirstOrDefaultAsync(c => c.Cid == cid, cancellationToken)
        ?? throw new InvalidOperationException($"Crop {cid} not found.");

    private static void ValidateDate(CROP crop, DateOnly date)
    {
        if (crop.Start_date is null)
            throw new InvalidOperationException($"Crop {crop.Cid} has no start date.");

        if (date < crop.Start_date.Value)
            throw new InvalidOperationException(
                $"Date {date:yyyy-MM-dd} is before planting date {crop.Start_date:yyyy-MM-dd}.");

    }

    private static IrrigationDayDto MapDayDto(CROP crop, CROP_WATER_BALANCE_LOG log)
    {
        var area = crop.Area_size ?? 1m;
        var recommendedMm = log.Irrig_mm ?? 0m;
        var m3PerFeddan = Math.Round(recommendedMm * (decimal)MM_TO_M3_PER_FEDDAN, 2);
        var m3Field = Math.Round(m3PerFeddan * area, 2);
        var appliedLiters = log.Applied_mm.HasValue
            ? Math.Round(log.Applied_mm.Value * (decimal)MM_TO_M3_PER_FEDDAN * area * 1000m, 0)
            : (decimal?)null;

        return new IrrigationDayDto
        {
            Cid = crop.Cid,
            Date = log.Date,
            PlantName = crop.PidNavigation?.Name ?? "محصول",
            StageName = log.StageName,
            SoilType = crop.Soil_type ?? crop.FarmNavigation?.Default_Soil_type ?? "loam",
            AreaFeddan = area,
            IsIrrigationDay = log.IsIrrigationDay,
            WasApplied = log.WasApplied,
            AppliedAt = log.AppliedAt,
            Recommended_m3_per_feddan = m3PerFeddan,
            Recommended_m3_field = m3Field,
            Recommended_Liters = log.Recommended_Liters ?? Math.Round(m3Field * 1000m, 0),
            Applied_Liters = appliedLiters,
            ET0_mm = log.ET0_mm ?? 0m,
            Kc = log.Kc ?? 0m,
            ETc_mm = log.ETc_mm ?? 0m,
            EffRain_mm = log.EffRain_mm ?? 0m,
            TAW_mm = log.TAW_mm ?? 0m,
            RAW_mm = log.RAW_mm ?? 0m,
            DeplStart_mm = log.DeplStart_mm ?? 0m,
            DeplAfterEt_mm = log.DeplAfterEt_mm ?? 0m,
            DeplEnd_mm = log.DeplEnd_mm ?? 0m,
            Recommended_mm = recommendedMm,
            Applied_mm = log.Applied_mm
        };
    }

    private static IrrigationCalendarDayDto MapCalendarDay(CROP_WATER_BALANCE_LOG log, decimal areaFeddan) =>
        new()
        {
            Date = log.Date,
            StageName = log.StageName,
            IsIrrigationDay = log.IsIrrigationDay,
            WasApplied = log.WasApplied,
            Recommended_Liters = log.Recommended_Liters ?? 0m,
            Applied_Liters = log.Applied_mm.HasValue && log.Applied_mm > 0
                ? Math.Round(log.Applied_mm.Value * (decimal)MM_TO_M3_PER_FEDDAN * areaFeddan * 1000m, 0)
                : null,
            DeplEnd_mm = log.DeplEnd_mm ?? 0m,
            ETc_mm = log.ETc_mm ?? 0m,
            EffRain_mm = log.EffRain_mm ?? 0m
        };

    private static IrrigationRecommendationDto ToLegacyRecommendation(IrrigationDayDto dto) =>
        new()
        {
            Cid = dto.Cid,
            Date = dto.Date,
            PlantName = dto.PlantName ?? "محصول",
            StageName = dto.StageName ?? string.Empty,
            SoilType = dto.SoilType ?? "loam",
            AreaFeddan = dto.AreaFeddan,
            IsIrrigationDay = dto.IsIrrigationDay,
            Recommended_m3_per_feddan = dto.Recommended_m3_per_feddan,
            Recommended_m3_field = dto.Recommended_m3_field,
            Recommended_Liters_field = dto.Recommended_Liters,
            ET0_mm = dto.ET0_mm,
            Kc = dto.Kc,
            ETc_mm = dto.ETc_mm,
            EffRain_mm = dto.EffRain_mm,
            TAW_mm = dto.TAW_mm,
            RAW_mm = dto.RAW_mm,
            DeplStart_mm = dto.DeplStart_mm,
            DeplAfterEt_mm = dto.DeplAfterEt_mm,
            DeplEnd_mm = dto.DeplEnd_mm,
            Irrig_mm = dto.Recommended_mm
        };

    private static IrrigationAdviceReportDto CreatePlaceholderAdvice(IrrigationDayDto dto) =>
        new()
        {
            Summary = dto.IsIrrigationDay
                ? $"اليوم يوم ري لمحصول {dto.PlantName} في مرحلة {dto.StageName}."
                : $"لا حاجة للري اليوم لمحصول {dto.PlantName} — رطوبة التربة ضمن الحد الآمن.",
            Recommendation = dto.IsIrrigationDay
                ? $"يُوصى بري الحقل بكمية تقارب {dto.Recommended_m3_field} م³ ({dto.Recommended_Liters} لتر)."
                : "لا تُطبّق رية اليوم؛ راقب مستوى النضج الجاف غداً.",
            Timing = "يفضّل الري عند الفجر أو قبل الغروب لتقليل التبخر.",
            ApplicationTips = "وزّع المياه بانتظام على صفوف المحصول وتجنب الجريان السطحي.",
            SoilWeatherNotes = $"تربة {dto.SoilType} — استهلاك نباتي تقديري {dto.ETc_mm} مم مع أمطار فعّالة {dto.EffRain_mm} مم.",
            Warnings = "تعذّر توليد تقرير مفصّل حالياً — راجع الكميات المحسوبة في النظام."
        };

    private static double ResolveLatitude(CROP crop) =>
        (double?)(crop.FarmNavigation?.Latitude ?? crop.UidNavigation?.Latitude) ?? 30.0444;

    private static double ResolveLongitude(CROP crop) =>
        (double?)(crop.FarmNavigation?.Longitude ?? crop.UidNavigation?.Longitude) ?? 31.2357;

    private static decimal Round2(double value) => (decimal)Math.Round(value, 2);

    private sealed record DayComputation(
        double Et0,
        double Kc,
        double Etc,
        double EffRain,
        double TawMm,
        double RawMm,
        double DeplAfterEt,
        double DeplEnd,
        bool IsIrrigationDay,
        double RecommendedIrrigMm,
        decimal RecommendedLiters,
        string StageName,
        string SoilNote,
        decimal AreaFeddan);
}
