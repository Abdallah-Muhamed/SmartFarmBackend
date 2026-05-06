namespace Smart_Farm.Application.Abstractions;

public record LocationLookupResult(
    double Latitude,
    double Longitude,
    string? Governorate,
    string? City,
    string? AddressLine,
    string? DisplayName);

public interface ILocationGeocodingService
{
    Task<LocationLookupResult?> ReverseAsync(double latitude, double longitude, CancellationToken cancellationToken);
    Task<LocationLookupResult?> ForwardAsync(string query, CancellationToken cancellationToken);
}
