using Microsoft.EntityFrameworkCore;
using Smart_Farm.Application.Abstractions;
using Smart_Farm.Models;
using FarmTask = Smart_Farm.Models.Task;

namespace Smart_Farm.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that runs twice daily:
///  - 06:00 Egypt time → generates irrigation + weather-alert Tasks from forecast.
///  - 20:00 Egypt time → re-checks actual rain; adds correction Task if forecast rain never came.
/// </summary>
public sealed class WeatherTaskGeneratorService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<WeatherTaskGeneratorService> _logger;

    private const int EGYPT_UTC_OFFSET_HOURS = 2;   // UTC+2 (EET); change to 3 in summer if needed

    public WeatherTaskGeneratorService(IServiceProvider sp, ILogger<WeatherTaskGeneratorService> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Main loop: sleep until next scheduled run, then execute.
    // ─────────────────────────────────────────────────────────────────────────

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var (nextRun, runType) = ComputeNextRun();
            var delay = nextRun - DateTime.UtcNow;

            _logger.LogInformation(
                "WeatherTaskGenerator: next {RunType} run at {NextRun:yyyy-MM-dd HH:mm} UTC (in {Delay:hh\\:mm})",
                runType, nextRun, delay);

            try { await System.Threading.Tasks.Task.Delay(delay, stoppingToken); }
            catch (OperationCanceledException) { break; }

            try
            {
                if (runType == RunType.Morning)
                    await RunMorningAsync(stoppingToken);
                else
                    await RunEveningAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WeatherTaskGenerator {RunType} run failed", runType);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Morning run (06:00): generate Tasks from forecast
    // ─────────────────────────────────────────────────────────────────────────

    private async System.Threading.Tasks.Task RunMorningAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var db           = scope.ServiceProvider.GetRequiredService<Smart_Farm.Models.farContext>();
        var waterBalance = scope.ServiceProvider.GetRequiredService<IWaterBalanceService>();
        var weather      = scope.ServiceProvider.GetRequiredService<IWeatherProvider>();

        var today = EgyptToday();

        var crops = await db.CROPs
            .Include(c => c.PidNavigation)
            .Include(c => c.FarmNavigation)
            .Include(c => c.UidNavigation)
            .Where(c => c.Uid != null && c.Start_date != null && c.Pid != null)
            .ToListAsync(ct);

        var tasksToAdd = new List<FarmTask>();

        // Group crops by farm to make ONE weather call per farm location
        foreach (var farmGroup in crops.GroupBy(c => c.FarmId ?? -c.Uid!.Value))
        {
            var first = farmGroup.First();
            var lat = ResolveLatitude(first);
            var lon = ResolveLongitude(first);

            DailyWeather wx;
            try
            {
                wx = await weather.GetDailyAsync(lat, lon, today, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Weather fetch failed for farm group {Key}", farmGroup.Key);
                continue;
            }

            // ── Shared weather alerts (one per user per day) ──────────────────
            foreach (var uid in farmGroup.Select(c => c.Uid!.Value).Distinct())
            {
                await AddIfNotExistsAsync(tasksToAdd, db, uid, cid: null, today,
                    label:   "حرارة مرتفعة",
                    content: $"⚠️ درجة الحرارة القصوى اليوم {wx.Tmax_C:0}°C — احرص على رش المزروعات صباحاً وتوفير تهوية كافية.",
                    when:    wx.Tmax_C >= 38, ct);

                await AddIfNotExistsAsync(tasksToAdd, db, uid, cid: null, today,
                    label:   "أمطار متوقعة",
                    content: $"⛅ متوقع هطول {wx.Rain_mm:0.0}mm اليوم — قد لا يكون الري ضرورياً. سيتم التحقق مساءً من الواقع.",
                    when:    wx.Rain_mm >= 10, ct);

                var windKmh = (wx.Wind_mps ?? 0) * 3.6;
                await AddIfNotExistsAsync(tasksToAdd, db, uid, cid: null, today,
                    label:   "رياح قوية",
                    content: $"💨 رياح قوية {windKmh:0} km/h — تأجيل رش المبيدات والأسمدة الورقية.",
                    when:    windKmh >= 40, ct);
            }

            // ── Irrigation tasks per crop ─────────────────────────────────────
            foreach (var crop in farmGroup)
            {
                if (crop.Uid is null) continue;

                try
                {
                    var rec = await waterBalance.SyncDayAsync(crop.Cid, today, ct);

                    if (rec.IsIrrigationDay)
                    {
                        var farmName = crop.FarmNavigation?.Name ?? $"مزرعة {crop.FarmId}";
                        var label = $"ري {rec.PlantName} - {farmName}";
                        await AddIfNotExistsAsync(tasksToAdd, db, crop.Uid.Value, crop.Cid, today,
                            label:   label,
                            content: $"💧 {rec.PlantName} ({rec.StageName}) في {farmName} يحتاج ري اليوم.\n" +
                                     $"الكمية: {rec.Recommended_Liters:0} لتر ({rec.Recommended_m3_field:0.0} م³) لـ {rec.AreaFeddan} فدان.\n" +
                                     $"ET0={rec.ET0_mm}mm | Kc={rec.Kc} | أمطار فعالة={rec.EffRain_mm}mm",
                            when:    true, ct);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Water balance failed for crop {Cid}", crop.Cid);
                }
            }
        }

        if (tasksToAdd.Count > 0)
        {
            db.Tasks.AddRange(tasksToAdd);
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Morning run: added {Count} tasks for {Date}", tasksToAdd.Count, today);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Evening run (20:00): verify actual rain vs morning forecast
    // ─────────────────────────────────────────────────────────────────────────

    private async System.Threading.Tasks.Task RunEveningAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var db      = scope.ServiceProvider.GetRequiredService<Smart_Farm.Models.farContext>();
        var weather = scope.ServiceProvider.GetRequiredService<IWeatherProvider>();

        var today = EgyptToday();

        // Only check users who received a "أمطار متوقعة" task this morning
        var rainTaskUids = await db.Tasks
            .Where(t => t.Date == today && t.Label == "أمطار متوقعة")
            .Select(t => t.Uid!.Value)
            .Distinct()
            .ToListAsync(ct);

        if (!rainTaskUids.Any()) return;

        var tasksToAdd = new List<FarmTask>();

        foreach (var uid in rainTaskUids)
        {
            var farm = await db.FARMs
                .Where(f => f.Uid == uid && f.Latitude != null)
                .FirstOrDefaultAsync(ct);

            var user = await db.USERs.FindAsync(new object[] { uid }, ct);

            var lat = (double?)(farm?.Latitude ?? user?.Latitude) ?? 30.0444;
            var lon = (double?)(farm?.Longitude ?? user?.Longitude) ?? 31.2357;

            DailyWeather actual;
            try { actual = await weather.GetDailyAsync(lat, lon, today, ct); }
            catch { continue; }

            // If actual rain < 5mm but morning forecast ≥ 10mm → correction needed
            if (actual.Rain_mm < 5)
            {
                await AddIfNotExistsAsync(tasksToAdd, db, uid, cid: null, today,
                    label:   "تصحيح توقعات الأمطار",
                    content: $"⚠️ كان متوقعاً هطول أمطار اليوم ولكن لم تمطر (فعلي: {actual.Rain_mm:0.0}mm).\n" +
                             $"تحقق من محاصيلك وقرر إذا كان الري ضرورياً الآن.",
                    when:    true, ct);
            }
        }

        if (tasksToAdd.Count > 0)
        {
            db.Tasks.AddRange(tasksToAdd);
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Evening run: added {Count} correction tasks for {Date}", tasksToAdd.Count, today);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static async System.Threading.Tasks.Task AddIfNotExistsAsync(
        List<FarmTask> buffer,
        Smart_Farm.Models.farContext db,
        int uid,
        int? cid,
        DateOnly date,
        string label,
        string content,
        bool when,
        CancellationToken ct)
    {
        if (!when) return;

        // Check buffer first (avoid double-add within the same run)
        if (buffer.Any(t => t.Uid == uid && t.Date == date && t.Label == label)) return;

        // Check database
        if (await db.Tasks.AnyAsync(t => t.Uid == uid && t.Date == date && t.Label == label, ct)) return;

        buffer.Add(new FarmTask
        {
            Uid       = uid,
            Cid       = cid,
            Date      = date,
            Label     = label,
            Content   = content,
            State     = "pending",
            CreatedAt = DateTime.UtcNow
        });
    }

    private static (DateTime nextRun, RunType runType) ComputeNextRun()
    {
        var nowUtc    = DateTime.UtcNow;
        var nowEgypt  = nowUtc.AddHours(EGYPT_UTC_OFFSET_HOURS);

        var morningEgypt = nowEgypt.Date.AddHours(6);
        var eveningEgypt = nowEgypt.Date.AddHours(17);

        if (nowEgypt < morningEgypt)
            return (nowUtc + (morningEgypt - nowEgypt), RunType.Morning);

        if (nowEgypt < eveningEgypt)
            return (nowUtc + (eveningEgypt - nowEgypt), RunType.Evening);

        // Both passed today → schedule morning tomorrow
        var tomorrowMorning = morningEgypt.AddDays(1);
        return (nowUtc + (tomorrowMorning - nowEgypt), RunType.Morning);
    }

    private static DateOnly EgyptToday() =>
        DateOnly.FromDateTime(DateTime.UtcNow.AddHours(EGYPT_UTC_OFFSET_HOURS));

    private static double ResolveLatitude(CROP crop) =>
        (double?)(crop.FarmNavigation?.Latitude ?? crop.UidNavigation?.Latitude) ?? 30.0444;

    private static double ResolveLongitude(CROP crop) =>
        (double?)(crop.FarmNavigation?.Longitude ?? crop.UidNavigation?.Longitude) ?? 31.2357;

    private enum RunType { Morning, Evening }
}
