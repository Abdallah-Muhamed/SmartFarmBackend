namespace Smart_Farm.DTOS;

public class Belong_to
{
    public int Cid { get; set; }
    public required string CropName { get; set; }
    public int Pid { get; set; }
    public required string PlantName { get; set; }
    public int? PlantCount { get; set; }
    public DateOnly? SowTime { get; set; }
    public DateOnly? HarvestTime { get; set; }
}
