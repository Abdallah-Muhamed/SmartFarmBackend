using Smart_Farm.DTOS;

namespace Smart_Farm.Application.Abstractions;

/// <summary>Internal shape used by the Groq irrigation advice generator.</summary>
public class IrrigationRecommendationDto
{
    public int Cid { get; set; }
    public DateOnly Date { get; set; }
    public string PlantName { get; set; } = default!;
    public string StageName { get; set; } = default!;
    public string SoilType { get; set; } = default!;
    public decimal AreaFeddan { get; set; }
    public bool IsIrrigationDay { get; set; }
    public decimal Recommended_m3_per_feddan { get; set; }
    public decimal Recommended_m3_field { get; set; }
    public decimal Recommended_Liters_field { get; set; }
    public decimal ET0_mm { get; set; }
    public decimal Kc { get; set; }
    public decimal ETc_mm { get; set; }
    public decimal EffRain_mm { get; set; }
    public decimal TAW_mm { get; set; }
    public decimal RAW_mm { get; set; }
    public decimal DeplStart_mm { get; set; }
    public decimal DeplAfterEt_mm { get; set; }
    public decimal DeplEnd_mm { get; set; }
    public decimal Irrig_mm { get; set; }
    public IrrigationAdviceReportDto? Reasoning { get; set; }
}
