namespace Smart_Farm.DTOS;

public class IrrigationStageDTO
{
    public int Sid { get; set; }

    public required string Name_stage { get; set; }

    public int? Stage_order { get; set; }

    public required string Description { get; set; }

    public int? Cid { get; set; }

    public required string CropName { get; set; }
}
