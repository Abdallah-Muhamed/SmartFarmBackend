namespace Smart_Farm.Common;

public static class DateTimeUtc
{
    /// <summary>Marks stored UTC values so JSON serializes as ISO 8601 with Z (e.g. 2026-06-01T12:00:00Z).</summary>
    public static DateTime? Normalize(DateTime? value) =>
        value is null ? null : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
}
