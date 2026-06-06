namespace Smart_Farm.Common;

public static class FarmTime
{
    public const int EgyptUtcOffsetHours = 2;

    public static DateOnly EgyptToday() =>
        DateOnly.FromDateTime(DateTime.UtcNow.AddHours(EgyptUtcOffsetHours));
}
