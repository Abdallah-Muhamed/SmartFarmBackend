using System.Text.Json.Serialization;

namespace Smart_Farm.DTOS;

public class RegisterDTO
{
    [JsonPropertyName("first_name")]
    public required string First_name { get; set; }

    [JsonPropertyName("last_name")]
    public required string Last_name { get; set; }

    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("address_line")]
    public required string Address_line { get; set; }

    [JsonPropertyName("city_name")]
    public required string City_name { get; set; }

    [JsonPropertyName("latitude")]
    public decimal? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public decimal? Longitude { get; set; }

    [JsonPropertyName("role")]
    public required string Role { get; set; }

    [JsonPropertyName("password")]
    public required string Password { get; set; }

    [JsonPropertyName("phone")]
    public required string Phone { get; set; }
}
