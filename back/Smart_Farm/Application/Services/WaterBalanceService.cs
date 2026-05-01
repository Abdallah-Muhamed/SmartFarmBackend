using Microsoft.EntityFrameworkCore;
using Smart_Farm.Application.Abstractions;
using Smart_Farm.Models;

namespace Smart_Farm.Application.Services;

/// <summary>
/// Daily soil-water balance + irrigation recommendation.
/// Units:
///   - ET0, ETc, EffRain, Irrig, Depletion, TAW, RAW: millimetres (mm)
///   - PLANT_IRRIGATION_TEMPLATE.Water_amount: cubic metres per feddan (م³/فدان)
///   - 1 mm over 1 feddan = 4.2 m³  (1 feddan = 4200 m²)
/// </summary>
public class WaterBalanceService : IWaterBalanceService
{
    private readonly farContext _db;
    private readonly IWeatherProvider _weather;
    private readonly ILogger<WaterBalanceService> _logger;

    private const double FEDDAN_M2 = 4200.0;
    private const double MM_TO_M3_PER_FEDDAN = 4.2;
    private const double RAIN_RUNOFF_FACTOR = 0.8;      // 20% runoff assumption
    private const double IRRIG_EFFICIENCY = 0.85;       // drip/sprinkler default

    public WaterBalanceService(farContext db, IWeatherProvider weather, ILogger<WaterBalanceService> logger)
    {
        _db = db;
        _weather = weather;
        _logger = logger;
    }

    public async Task<IrrigationRecommendationDto> ComputeDailyAsync(
        int cid,
        DateOnly date,
        bool persist,
        CancellationToken cancellationToken)
    {
        // ─── 1. Load crop + plant + location ──────────────────────────────
        var crop = await _db.CROPs
            .Include(c => c.PidNavigation)
            .Include(c => c.UidNavigation)
            .Include(c => c.FarmNavigation)
            .FirstOrDefaultAsync(c => c.Cid == cid, cancellationToken)
            ?? throw new InvalidOperationException($"Crop {cid} not found.");

        if (crop.Pid is null)
            throw new InvalidOperationException($"Crop {cid} has no plant linked.");
        if (crop.Start_date is null)
            throw new InvalidOperationException($"Crop {cid} has no start date.");

        var plantName = crop.PidNavigation?.Name ?? "محصول";
        var soilType = crop.Soil_type
                       ?? crop.FarmNavigation?.Default_Soil_type
                       ?? "loam";
        var areaFeddan = crop.Area_size ?? 1m;

        // Prefer farm coordinates (more accurate per-field), fall back to user, then Cairo.
        var lat = (double?)(crop.FarmNavigation?.Latitude ?? crop.UidNavigation?.Latitude) ?? 30.0444;
        var lon = (double?)(crop.FarmNavigation?.Longitude ?? crop.UidNavigation?.Longitude) ?? 31.2357;

        // ─── 2. Determine current stage ───────────────────────────────────
        var stages = await _db.PLANT_STAGEs
            .Where(s => s.Pid == crop.Pid)
            .OrderBy(s => s.Stage_order)
            .ToListAsync(cancellationToken);

        if (stages.Count == 0)
            throw new InvalidOperationException($"No PLANT_STAGE rows for Pid={crop.Pid}.");

        var daysSinceStart = Math.Max(0, date.DayNumber - crop.Start_date!.Value.DayNumber);
        var currentStage = ResolveCurrentStage(stages, daysSinceStart);

        var template = await _db.PLANT_IRRIGATION_TEMPLATEs
            .FirstOrDefaultAsync(t => t.PSid == currentStage.PSid, cancellationToken);

        // ─── 3. Soil + crop constants ─────────────────────────────────────
        var (tawPerMetre, soilNote) = SoilLookup.GetTawPerMetre(soilType);

        double kc = (double?)template?.Kc ?? 1.00;
        double p = (double?)template?.p_fraction ?? 0.50;
        double zr = (double?)template?.Zr_m ?? 0.40;
        double tawMm = tawPerMetre * zr;
        double rawMm = p * tawMm;

        // ─── 4. Weather for the day + ET0 (Hargreaves) ────────────────────
        var wx = await _weather.GetDailyAsync(lat, lon, date, cancellationToken);
        double et0 = Hargreaves.Compute(wx.Tmin_C, wx.Tmax_C, lat, date);
        double etc = kc * et0;

        // ─── 5. Running depletion + rainfall ──────────────────────────────
        double deplStart = (double?)crop.Depletion_mm ?? 0.0;
        // Reset depletion if LastBalanceDate is very old (avoid unbounded drift).
        if (crop.LastBalanceDate is null || (date.DayNumber - crop.LastBalanceDate.Value.DayNumber) > 30)
            deplStart = 0;

        // Effective rain: cap at available storage (can't infiltrate more than the current deficit + small overflow buffer).
        double rawRain = Math.Max(0, wx.Rain_mm) * RAIN_RUNOFF_FACTOR;
        double effRain = Math.Min(rawRain, deplStart + tawMm - rawMm + rawMm); // cap at ~TAW
        effRain = Math.Min(effRain, deplStart + tawMm); // never exceeds TAW refilled
        if (effRain < 0) effRain = 0;

        double deplAfterEt = Math.Max(0, deplStart + etc - effRain);

        // ─── 6. Irrigation decision ───────────────────────────────────────
        bool isIrrigDay = deplAfterEt >= rawMm;
        double irrigMm = 0;
        double deplEnd = deplAfterEt;
        string reason;

        if (isIrrigDay)
        {
            // Refill to field capacity — but gross amount accounts for application efficiency.
            double netNeed = deplAfterEt;                    // mm required at the root zone
            double grossMm = netNeed / IRRIG_EFFICIENCY;     // mm to apply at surface
            irrigMm = Math.Round(grossMm, 2);
            deplEnd = 0;
            reason =
                $"Depletion {deplAfterEt:0.0} mm ≥ RAW {rawMm:0.0} mm ⇒ irrigation needed. "
                + $"Gross depth = {irrigMm:0.0} mm (net {netNeed:0.0} mm ÷ η {IRRIG_EFFICIENCY:0.00}).";
        }
        else
        {
            reason = $"Depletion {deplAfterEt:0.0} mm < RAW {rawMm:0.0} mm ⇒ no irrigation today.";
        }

        // ─── 7. Convert to م³/فدان and field totals ───────────────────────
        decimal m3PerFeddan = (decimal)Math.Round(irrigMm * MM_TO_M3_PER_FEDDAN, 2);
        decimal m3Field = Math.Round(m3PerFeddan * areaFeddan, 2);
        decimal litersField = Math.Round(m3Field * 1000m, 0);

        // ─── 8. Persist state + log ───────────────────────────────────────
        if (persist)
        {
            crop.Depletion_mm = (decimal)Math.Round(deplEnd, 2);
            crop.LastBalanceDate = date;

            _db.CROP_WATER_BALANCE_LOGs.Add(new CROP_WATER_BALANCE_LOG
            {
                Cid = cid,
                Date = date,
                ET0_mm = (decimal)Math.Round(et0, 2),
                Kc = (decimal)Math.Round(kc, 2),
                ETc_mm = (decimal)Math.Round(etc, 2),
                EffRain_mm = (decimal)Math.Round(effRain, 2),
                Irrig_mm = (decimal)Math.Round(irrigMm, 2),
                DeplStart_mm = (decimal)Math.Round(deplStart, 2),
                DeplEnd_mm = (decimal)Math.Round(deplEnd, 2),
                TAW_mm = (decimal)Math.Round(tawMm, 2),
                RAW_mm = (decimal)Math.Round(rawMm, 2),
                Note = soilNote
            });

            await _db.SaveChangesAsync(cancellationToken);
        }

        // ─── 9. Build DTO ─────────────────────────────────────────────────
        return new IrrigationRecommendationDto
        {
            Cid = cid,
            Date = date,
            PlantName = plantName,
            StageName = currentStage.Name_stage ?? $"Stage {currentStage.Stage_order}",
            SoilType = soilType,
            AreaFeddan = areaFeddan,

            IsIrrigationDay = isIrrigDay,
            Recommended_m3_per_feddan = m3PerFeddan,
            Recommended_m3_field = m3Field,
            Recommended_Liters_field = litersField,

            ET0_mm = (decimal)Math.Round(et0, 2),
            Kc = (decimal)Math.Round(kc, 2),
            ETc_mm = (decimal)Math.Round(etc, 2),
            EffRain_mm = (decimal)Math.Round(effRain, 2),
            TAW_mm = (decimal)Math.Round(tawMm, 2),
            RAW_mm = (decimal)Math.Round(rawMm, 2),
            DeplStart_mm = (decimal)Math.Round(deplStart, 2),
            DeplAfterEt_mm = (decimal)Math.Round(deplAfterEt, 2),
            DeplEnd_mm = (decimal)Math.Round(deplEnd, 2),
            Irrig_mm = (decimal)Math.Round(irrigMm, 2),
            Reasoning = reason
        };
    }

    private static PLANT_STAGE ResolveCurrentStage(List<PLANT_STAGE> stages, int daysSinceStart)
    {
        int cum = 0;
        foreach (var s in stages)
        {
            cum += s.Duration_days;
            if (daysSinceStart < cum)
                return s;
        }
        return stages[^1]; // past harvest → keep last stage
    }
}
