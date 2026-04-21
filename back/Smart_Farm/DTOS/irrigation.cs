namespace Smart_Farm.DTOS;

public class IrrigationDTO
{
    public int Iid { get; set; }

    public required string Irrigation_name { get; set; }

    public required string Description { get; set; }

    public required string Frequency_unit { get; set; }

    public int? Frequency_value { get; set; }

    public decimal? Water_amount { get; set; }

    public int? Sis { get; set; }

    public int? Cid { get; set; }

    public required string StageName { get; set; }

    public required string CropName { get; set; }
}
