namespace Smart_Farm.DTOS;

public class Grows_in
{
    public required string PlantName { get; set; }
    public required string SeasonName { get; set; }
    public required string Description { get; set; }
    public decimal? Rate { get; set; }
}
