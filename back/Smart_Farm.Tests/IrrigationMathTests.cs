using Smart_Farm.Application.Services;
using Smart_Farm.Models;
using Xunit;

namespace Smart_Farm.Tests;

public class IrrigationMathTests
{
    [Fact]
    public void Depletion_accumulates_daily_until_irrigation_threshold()
    {
        const double raw = 20;
        var depl = 0.0;

        for (var day = 1; day <= 3; day++)
        {
            depl = IrrigationMath.DepletionAfterEt(depl, etc: 5, effRain: 0);
            Assert.False(IrrigationMath.IsIrrigationDay(depl, raw));
        }

        depl = IrrigationMath.DepletionAfterEt(depl, etc: 5, effRain: 0);
        Assert.True(IrrigationMath.IsIrrigationDay(depl, raw));
        Assert.Equal(20, depl);
    }

    [Fact]
    public void Full_irrigation_resets_depletion_to_zero()
    {
        var end = IrrigationMath.DepletionEndAfterApplication(
            deplAfterEt: 25m,
            appliedMm: 30m,
            recommendedMm: 29.41m);

        Assert.Equal(0m, end);
    }

    [Fact]
    public void Skipped_irrigation_keeps_depletion_after_et()
    {
        var end = IrrigationMath.DepletionEndAfterApplication(
            deplAfterEt: 25m,
            appliedMm: 0m,
            recommendedMm: 29.41m);

        Assert.Equal(25m, end);
    }

    [Fact]
    public void Rain_reduces_depletion()
    {
        var effRain = IrrigationMath.EffectiveRain(rainMm: 10, deplStart: 15, tawMm: 60);
        var depl = IrrigationMath.DepletionAfterEt(deplStart: 15, etc: 5, effRain);

        Assert.Equal(8, effRain);
        Assert.Equal(12, depl);
    }

    [Fact]
    public void Liters_to_mm_roundtrip_is_consistent()
    {
        const decimal area = 2m;
        const decimal liters = 8400m; // 2 feddans × 4.2 m³/feddan × 1000
        var mm = IrrigationMath.LitersToMm(liters, area);
        Assert.Equal(1m, mm);
    }

    [Fact]
    public void ResolveCurrentStage_picks_correct_stage()
    {
        var stages = new List<PLANT_STAGE>
        {
            new() { PSid = 1, Duration_days = 4, Stage_order = 1, Name_stage = "A" },
            new() { PSid = 2, Duration_days = 7, Stage_order = 2, Name_stage = "B" },
            new() { PSid = 3, Duration_days = 11, Stage_order = 3, Name_stage = "C" },
        };

        Assert.Equal("A", IrrigationMath.ResolveCurrentStage(stages, 0).Name_stage);
        Assert.Equal("A", IrrigationMath.ResolveCurrentStage(stages, 3).Name_stage);
        Assert.Equal("B", IrrigationMath.ResolveCurrentStage(stages, 4).Name_stage);
        Assert.Equal("C", IrrigationMath.ResolveCurrentStage(stages, 99).Name_stage);
    }
}
