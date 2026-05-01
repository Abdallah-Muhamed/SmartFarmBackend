namespace Smart_Farm.Application.Services;

/// <summary>
/// Total Available Water per metre of root zone, by soil texture class.
/// Values in mm/m (FAO-56 Table 19 – typical midpoints).
/// </summary>
public static class SoilLookup
{
    public static (double TawPerMetre_mm, string Note) GetTawPerMetre(string? soilType)
    {
        var s = (soilType ?? string.Empty).Trim().ToLowerInvariant();

        // Arabic + English aliases.
        if (Contains(s, "sand", "رمل", "رملى", "رملي"))
            return (80, "Sandy soil: low water-holding capacity (≈ 80 mm/m).");

        if (Contains(s, "loamy sand", "sandy loam", "رملي طيني", "طميي رملي"))
            return (120, "Sandy loam: moderate capacity (≈ 120 mm/m).");

        if (Contains(s, "loam", "طمي", "طميي", "لوام"))
            return (150, "Loam: balanced capacity (≈ 150 mm/m).");

        if (Contains(s, "silt", "سلت", "غرين"))
            return (170, "Silty loam: high capacity (≈ 170 mm/m).");

        if (Contains(s, "clay loam", "طيني طمي", "طيني لوام"))
            return (180, "Clay loam: high capacity (≈ 180 mm/m).");

        if (Contains(s, "clay", "طين", "طيني"))
            return (200, "Clay: highest capacity but slow infiltration (≈ 200 mm/m).");

        // Default = loam.
        return (150, "Unknown soil type; defaulted to loam (≈ 150 mm/m).");
    }

    private static bool Contains(string src, params string[] needles)
    {
        foreach (var n in needles)
            if (!string.IsNullOrEmpty(n) && src.Contains(n, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }
}
