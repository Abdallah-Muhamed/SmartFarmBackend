using Smart_Farm.Models;

namespace Smart_Farm.Application.Services;

/// <summary>Pure irrigation calculations — no I/O, safe to unit test.</summary>
public static class IrrigationMath
{
    public const double IrrigEfficiency = 0.85;
    public const double RainRunoffFactor = 0.8;
    public const double MmToM3PerFeddan = 4.2;

    public static double EffectiveRain(double rainMm, double deplStart, double tawMm) =>
        Math.Min(Math.Max(0, rainMm) * RainRunoffFactor, Math.Max(0, deplStart + tawMm));

    public static double DepletionAfterEt(double deplStart, double etc, double effRain) =>
        Math.Max(0, deplStart + etc - effRain);

    public static bool IsIrrigationDay(double deplAfterEt, double rawMm) =>
        deplAfterEt >= rawMm;

    public static double RecommendedIrrigationMm(double deplAfterEt, bool isIrrigationDay) =>
        isIrrigationDay ? Math.Round(deplAfterEt / IrrigEfficiency, 2) : 0;

    public static decimal DepletionEndAfterApplication(decimal deplAfterEt, decimal appliedMm, decimal recommendedMm)
    {
        if (appliedMm <= 0) return deplAfterEt;
        if (recommendedMm > 0 && appliedMm >= recommendedMm * 0.95m) return 0m;
        var netApplied = appliedMm * (decimal)IrrigEfficiency;
        return Math.Max(0m, deplAfterEt - netApplied);
    }

    public static decimal LitersToMm(decimal liters, decimal areaFeddan)
    {
        if (areaFeddan <= 0) areaFeddan = 1m;
        var m3PerFeddan = (liters / 1000m) / areaFeddan;
        return Math.Round(m3PerFeddan / (decimal)MmToM3PerFeddan, 2);
    }

    public static PLANT_STAGE ResolveCurrentStage(IReadOnlyList<PLANT_STAGE> stages, int daysSinceStart)
    {
        int cum = 0;
        foreach (var s in stages)
        {
            cum += s.Duration_days;
            if (daysSinceStart < cum)
                return s;
        }
        return stages[^1];
    }
}
