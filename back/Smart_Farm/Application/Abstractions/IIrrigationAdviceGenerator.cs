using Smart_Farm.DTOS;

namespace Smart_Farm.Application.Abstractions;

public class IrrigationAdviceInput
{
    public required IrrigationRecommendationDto Recommendation { get; init; }
    public double Rain_mm { get; init; }
    public double Tmin_C { get; init; }
    public double Tmax_C { get; init; }
}

public interface IIrrigationAdviceGenerator
{
    Task<IrrigationAdviceReportDto> GenerateArabicAdviceAsync(
        IrrigationAdviceInput input,
        CancellationToken cancellationToken);
}
