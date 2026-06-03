namespace Smart_Farm.Application.Abstractions;

public sealed class PlantSpeciesIdentificationResult
{
    public required string BestMatch { get; init; }
    public string? ScientificName { get; init; }
    public string? ArabicName { get; init; }
    public IReadOnlyList<string> CommonNames { get; init; } = [];
    public double Confidence { get; init; }
    public required string RawResponse { get; init; }
}
