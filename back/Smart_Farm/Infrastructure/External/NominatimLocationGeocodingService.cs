using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Smart_Farm.Application.Abstractions;

namespace Smart_Farm.Infrastructure.External;

public class NominatimLocationGeocodingService(
    IHttpClientFactory httpClientFactory,
    ILogger<NominatimLocationGeocodingService> logger) : ILocationGeocodingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<LocationLookupResult?> ReverseAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        var url = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={latitude.ToString(CultureInfo.InvariantCulture)}&lon={longitude.ToString(CultureInfo.InvariantCulture)}&addressdetails=1";

        try
        {
            var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Reverse geocoding failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<NominatimReverseResponse>(stream, JsonOptions, cancellationToken);
            if (payload is null)
                return null;

            return MapResult(payload.Latitude, payload.Longitude, payload);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Reverse geocoding failed for {Latitude},{Longitude}", latitude, longitude);
            return null;
        }
    }

    public async Task<LocationLookupResult?> ForwardAsync(string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            return null;

        var client = CreateClient();
        var url = $"https://nominatim.openstreetmap.org/search?format=jsonv2&limit=1&addressdetails=1&q={Uri.EscapeDataString(query)}";

        try
        {
            var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Forward geocoding failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<List<NominatimSearchResponse>>(stream, JsonOptions, cancellationToken);
            var first = payload?.FirstOrDefault();
            if (first is null)
                return null;

            return MapResult(first.Latitude, first.Longitude, first);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Forward geocoding failed for query {Query}", query);
            return null;
        }
    }

    private HttpClient CreateClient()
    {
        var client = httpClientFactory.CreateClient("Nominatim");
        client.Timeout = TimeSpan.FromSeconds(20);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("SmartFarmBackend/1.0");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        return client;
    }

    private static LocationLookupResult MapResult(double latitude, double longitude, INominatimPayload payload)
    {
        var address = payload.Address;
        var governorate = address?.State ?? address?.County;
        var city = address?.City ?? address?.Town ?? address?.Village ?? address?.Municipality ?? address?.County;
        var addressLine = payload.DisplayName ?? address?.DisplayName;

        return new LocationLookupResult(
            latitude,
            longitude,
            governorate,
            city,
            addressLine,
            payload.DisplayName);
    }

    private interface INominatimPayload
    {
        double Latitude { get; }
        double Longitude { get; }
        string? DisplayName { get; }
        NominatimAddress? Address { get; }
    }

    private sealed class NominatimReverseResponse : INominatimPayload
    {
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("lat")]
        public string? Lat { get; set; }

        [JsonPropertyName("lon")]
        public string? Lon { get; set; }

        public double Latitude => ParseDouble(Lat);
        public double Longitude => ParseDouble(Lon);

        public NominatimAddress? Address { get; set; }
    }

    private sealed class NominatimSearchResponse : INominatimPayload
    {
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("lat")]
        public string? Lat { get; set; }

        [JsonPropertyName("lon")]
        public string? Lon { get; set; }

        public double Latitude => ParseDouble(Lat);
        public double Longitude => ParseDouble(Lon);

        public NominatimAddress? Address { get; set; }
    }

    private sealed class NominatimAddress
    {
        public string? State { get; set; }
        public string? County { get; set; }
        public string? City { get; set; }
        public string? Town { get; set; }
        public string? Village { get; set; }
        public string? Municipality { get; set; }
        public string? DisplayName { get; set; }
    }

    private static double ParseDouble(string? value)
        => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0d;
}
