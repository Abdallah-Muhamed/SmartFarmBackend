namespace Smart_Farm.Application.Abstractions;

public class IrrigationRecommendationDto
{
    public int Cid { get; set; }
    public DateOnly Date { get; set; }
    public string PlantName { get; set; } = default!;
    public string StageName { get; set; } = default!;
    public string SoilType { get; set; } = default!;
    public decimal AreaFeddan { get; set; }

    public bool IsIrrigationDay { get; set; }

    // Final recommendation.
    public decimal Recommended_m3_per_feddan { get; set; }
    public decimal Recommended_m3_field { get; set; }
    public decimal Recommended_Liters_field { get; set; }

    // Diagnostics — the numbers the recommendation was built from.
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

    public string Reasoning { get; set; } = string.Empty;
}

public interface IWaterBalanceService
{
    Task<IrrigationRecommendationDto> ComputeDailyAsync(
        int cid,
        DateOnly date,
        bool persist,
        CancellationToken cancellationToken);
}
