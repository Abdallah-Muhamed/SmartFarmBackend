namespace Smart_Farm.DTOS;

public class CreateFarmDto
{
    public string Name { get; set; } = default!;
    public string? LocationQuery { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? Address_line { get; set; }
    public decimal? Area_size { get; set; }
    public string? Default_Soil_type { get; set; }
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
