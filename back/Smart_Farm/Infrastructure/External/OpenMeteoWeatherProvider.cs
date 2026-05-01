using System.Globalization;
using System.Text.Json;
using Smart_Farm.Application.Abstractions;

namespace Smart_Farm.Infrastructure.External;

public class OpenMeteoWeatherProvider : IWeatherProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenMeteoWeatherProvider> _logger;

    public OpenMeteoWeatherProvider(IHttpClientFactory httpClientFactory, ILogger<OpenMeteoWeatherProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<DailyWeather> GetDailyAsync(
        double latitude,
        double longitude,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        // Open-Meteo: historical/archive for past, forecast for today/future.
        // We use the forecast endpoint which gracefully supports a single-day window including today.
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);
        var d = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var url = $"https://api.open-meteo.com/v1/forecast"
                + $"?latitude={lat}&longitude={lon}"
                + $"&daily=temperature_2m_max,temperature_2m_min,precipitation_sum,relative_humidity_2m_mean,wind_speed_10m_max"
                + $"&timezone=auto&start_date={d}&end_date={d}";

        var client = _httpClientFactory.CreateClient("OpenMeteo");
        client.Timeout = TimeSpan.FromSeconds(20);

        try
        {
            using var resp = await client.GetAsync(url, cancellationToken);
            resp.EnsureSuccessStatusCode();

            await using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var daily = doc.RootElement.GetProperty("daily");

            double tmax = FirstNumber(daily, "temperature_2m_max") ?? 25;
            double tmin = FirstNumber(daily, "temperature_2m_min") ?? 15;
            double rain = FirstNumber(daily, "precipitation_sum") ?? 0;
            double? rh = FirstNumber(daily, "relative_humidity_2m_mean");
            double? wind_kmh = FirstNumber(daily, "wind_speed_10m_max");
            double? wind_mps = wind_kmh.HasValue ? wind_kmh.Value / 3.6 : (double?)null;

            return new DailyWeather(date, tmin, tmax, rain, rh, wind_mps);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Open-Meteo fetch failed for {Lat},{Lon} on {Date}. Falling back to defaults.", lat, lon, d);
            // Safe fallback so the recommendation pipeline still works offline.
            return new DailyWeather(date, 18, 30, 0, 55, 2.0);
        }
    }

    private static double? FirstNumber(JsonElement daily, string key)
    {
        if (!daily.TryGetProperty(key, out var arr) || arr.ValueKind != JsonValueKind.Array || arr.GetArrayLength() == 0)
            return null;

        var first = arr[0];
        if (first.ValueKind == JsonValueKind.Number && first.TryGetDouble(out var d))
            return d;
        return null;
    }
}
