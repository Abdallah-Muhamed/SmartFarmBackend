namespace Smart_Farm.Application.Abstractions;

public sealed class PlantDiseasePredictionResult
{
    public required string DiseaseName { get; init; }
    public double Confidence { get; init; }
    public required string RawResponse { get; init; }
}
