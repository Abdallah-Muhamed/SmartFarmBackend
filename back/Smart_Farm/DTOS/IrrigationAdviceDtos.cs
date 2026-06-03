namespace Smart_Farm.DTOS;

/// <summary>Structured Arabic irrigation advice from Groq (maps to <c>reasoning</c> in the API).</summary>
public class IrrigationAdviceReportDto
{
    public string Summary { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string Timing { get; set; } = string.Empty;
    public string ApplicationTips { get; set; } = string.Empty;
    public string SoilWeatherNotes { get; set; } = string.Empty;
    public string Warnings { get; set; } = string.Empty;
}
