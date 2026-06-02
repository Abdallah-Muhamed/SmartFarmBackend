using Microsoft.EntityFrameworkCore;

namespace Smart_Farm.DTOS;

// Structured report from Groq
public class GroqReportDto
{
    public string Disease    { get; set; } = string.Empty;
    public string Symptoms   { get; set; } = string.Empty;
    public string Causes     { get; set; } = string.Empty;
    public string Treatment  { get; set; } = string.Empty;
    public string Prevention { get; set; } = string.Empty;
}

// POST /api/AIdiagnoses and GET list/detail
public class DiagnoseFullResultDto
{
    public int ADid { get; set; }
    public DateTime? DiagnosisDate { get; set; }
    public double? Confidence { get; set; }
    public int? Did { get; set; }
    public int? Cid { get; set; }
    public string? plant_image { get; set; }
    public GroqReportDto? Report { get; set; }
}

public class UpdateAIDiagnosisRequestDto
{
    public DateTime? DiagnosisDate { get; set; }
    public required string Result { get; set; }
    public int? Did { get; set; }
    public int? Cid { get; set; }
}

public class DiagnoseResultDto
{
    public int Id { get; set; }
    public required string Disease { get; set; }
    public double? Confidence { get; set; }
    public string? PlantName { get; set; }
    public bool Saved { get; set; }
    public string? plant_image { get; set; }
}

public class MyDiagnosisCountDto
{
    public int UserId { get; set; }
    public int DiagnosisCount { get; set; }
}
