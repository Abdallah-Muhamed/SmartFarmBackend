using System.Text.Json.Serialization;

namespace Smart_Farm.DTOS;

public class LoginDTO
{
    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("password")]
    public required string Password { get; set; }
}
