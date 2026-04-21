namespace Smart_Farm.DTOS;

public class AIDiagnosisResponseDto
{
    public int ADid { get; set; }
    public DateOnly? DiagnosisDate { get; set; }
    public required string Result { get; set; }
    public int? Did { get; set; }
    public int? Cid { get; set; }
    public string? DiseaseName { get; set; }
}

public class UpdateAIDiagnosisRequestDto
{
    public DateOnly? DiagnosisDate { get; set; }
    public required string Result { get; set; }
    public int? Did { get; set; }
    public int? Cid { get; set; }
}

public class DiagnoseResultDto
{
    public int Id { get; set; }
    public required string Disease { get; set; }
    public double Confidence { get; set; }
    public bool Saved { get; set; }
}
