namespace Smart_Farm.Application.Abstractions;

public interface IPlantSpeciesIdentifier
{
    Task<PlantSpeciesIdentificationResult> IdentifyAsync(
        Stream imageStream,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken);
}
