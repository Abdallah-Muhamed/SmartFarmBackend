namespace Smart_Farm.Application.Services;

/// <summary>
/// FAO-56 Hargreaves–Samani reference evapotranspiration (ET0) in mm/day.
///   ET0 = 0.0023 × (Tmean + 17.8) × (Tmax − Tmin)^0.5 × Ra
/// Ra (extraterrestrial radiation) is computed from latitude and day-of-year
/// in MJ/m²/day, then converted to equivalent mm/day via 1/2.45.
/// </summary>
public static class Hargreaves
{
    public static double Compute(double tmin_c, double tmax_c, double latitude_deg, DateOnly date)
    {
        if (tmax_c < tmin_c) (tmin_c, tmax_c) = (tmax_c, tmin_c);

        double tmean = (tmin_c + tmax_c) / 2.0;
        double ra_mm = ExtraterrestrialRadiation_mmPerDay(latitude_deg, date.DayOfYear);
        double dt = Math.Max(0.1, tmax_c - tmin_c);

        double et0 = 0.0023 * (tmean + 17.8) * Math.Sqrt(dt) * ra_mm;
        return Math.Max(0, et0);
    }

    private static double ExtraterrestrialRadiation_mmPerDay(double latitude_deg, int dayOfYear)
    {
        // FAO-56 Eq. 21, 22, 23, 24, 25
        double phi = latitude_deg * Math.PI / 180.0;                      // radians
        double dr  = 1.0 + 0.033 * Math.Cos(2 * Math.PI * dayOfYear / 365.0);
        double delta = 0.409 * Math.Sin(2 * Math.PI * dayOfYear / 365.0 - 1.39);

        double cosOmegaS = -Math.Tan(phi) * Math.Tan(delta);
        cosOmegaS = Math.Max(-1.0, Math.Min(1.0, cosOmegaS));
        double omegaS = Math.Acos(cosOmegaS);

        const double Gsc = 0.0820; // solar constant MJ/m²/min
        double ra_mj = (24.0 * 60.0 / Math.PI) * Gsc * dr
                     * (omegaS * Math.Sin(phi) * Math.Sin(delta)
                        + Math.Cos(phi) * Math.Cos(delta) * Math.Sin(omegaS));

        // Convert MJ/m²/day → mm/day (equivalent evaporation) via 1/λ ≈ 0.408
        return ra_mj * 0.408;
    }
}
