namespace Smart_Farm.DTOS;

public class SeasonDTO
{
    public int Sid { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required List<string> Plants { get; set; }
}
