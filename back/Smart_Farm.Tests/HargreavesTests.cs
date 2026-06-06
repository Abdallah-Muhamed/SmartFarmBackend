using Smart_Farm.Application.Services;
using Xunit;

namespace Smart_Farm.Tests;

public class HargreavesTests
{
    [Fact]
    public void Compute_returns_positive_et0_for_typical_summer_day()
    {
        var et0 = Hargreaves.Compute(tmin_c: 22, tmax_c: 35, latitude_deg: 30.0, new DateOnly(2026, 6, 15));
        Assert.True(et0 > 3);
        Assert.True(et0 < 15);
    }

    [Fact]
    public void Compute_swaps_tmin_tmax_when_inverted()
    {
        var normal = Hargreaves.Compute(22, 35, 30, new DateOnly(2026, 6, 15));
        var inverted = Hargreaves.Compute(35, 22, 30, new DateOnly(2026, 6, 15));
        Assert.Equal(normal, inverted, precision: 3);
    }
}
