using System.Globalization;
using System.Text;
using Smart_Farm.Application.Abstractions;

namespace Smart_Farm.Common;

public static class PlantNameMatcher
{
    public static bool Matches(string? expectedPlantName, PlantSpeciesIdentificationResult identified)
    {
        if (string.IsNullOrWhiteSpace(expectedPlantName))
            return false;

        var expected = Normalize(expectedPlantName);
        if (expected.Length == 0)
            return false;

        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Add(string? value)
        {
            var n = Normalize(value);
            if (n.Length > 0)
                candidates.Add(n);
        }

        Add(identified.ArabicName);
        Add(identified.ScientificName);
        Add(identified.BestMatch);

        foreach (var name in identified.CommonNames)
            Add(name);

        foreach (var candidate in candidates)
        {
            if (candidate == expected)
                return true;

            if (candidate.Contains(expected, StringComparison.OrdinalIgnoreCase)
                || expected.Contains(candidate, StringComparison.OrdinalIgnoreCase))
                return true;

            if (SharesSignificantToken(expected, candidate))
                return true;
        }

        return false;
    }

    public static string DisplayName(PlantSpeciesIdentificationResult identified) =>
        !string.IsNullOrWhiteSpace(identified.ArabicName)
            ? identified.ArabicName
            : identified.CommonNames.FirstOrDefault()
              ?? identified.BestMatch;

    private static bool SharesSignificantToken(string a, string b)
    {
        foreach (var token in a.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.Length < 3)
                continue;

            if (b.Contains(token, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var s = value.Trim().ToLowerInvariant();
        s = s.Replace('أ', 'ا').Replace('إ', 'ا').Replace('آ', 'ا').Replace('ى', 'ي').Replace('ة', 'ه');
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
