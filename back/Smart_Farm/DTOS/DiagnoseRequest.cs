namespace Smart_Farm.DTOS;

public class DiagnoseRequest
{
    public IFormFile? Image { get; set; }

    /// <summary>Crop id (required). Missing → 400; unknown id → 404.</summary>
    public int Cid { get; set; }
}
