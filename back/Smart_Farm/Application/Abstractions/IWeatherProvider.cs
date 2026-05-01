namespace Smart_Farm.Application.Abstractions;

public record DailyWeather(
    DateOnly Date,
    double Tmin_C,
    double Tmax_C,
    double Rain_mm,
    double? RH_pct,
    double? Wind_mps);

public interface IWeatherProvider
{
    Task<DailyWeather> GetDailyAsync(
        double latitude,
        double longitude,
        DateOnly date,
        CancellationToken cancellationToken);
}
