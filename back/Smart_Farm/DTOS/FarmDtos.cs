using System.Text.Json.Serialization;

namespace Smart_Farm.DTOS;

public class CreateFarmDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("locationQuery")]
    public string? LocationQuery { get; set; }

    [JsonPropertyName("latitude")]
    public decimal? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public decimal? Longitude { get; set; }

    [JsonPropertyName("governorate")]
    public string? Governorate { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("address_line")]
    public string? Address_line { get; set; }

    [JsonPropertyName("area_size")]
    public decimal? Area_size { get; set; }

    [JsonPropertyName("area")]
    public decimal? Area
    {
        get => Area_size;
        set => Area_size = value;
    }

    [JsonPropertyName("default_Soil_type")]
    public string? Default_Soil_type { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

public class UpdateFarmDto : CreateFarmDto { }

public class FarmResponseDto
{
    public int FarmId { get; set; }
    public string Name { get; set; } = default!;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? Address_line { get; set; }
    public decimal? Area_size { get; set; }
    public string? Default_Soil_type { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Uid { get; set; }
    public int CropCount { get; set; }
}
