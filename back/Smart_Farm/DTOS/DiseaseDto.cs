namespace Smart_Farm.DTOS;

public class DiseaseDto
{
    public int Did { get; set; }
    public required string Name { get; set; }
    public required string Cause { get; set; }
    public required string Symptoms { get; set; }
    public required string Treatment { get; set; }
}
