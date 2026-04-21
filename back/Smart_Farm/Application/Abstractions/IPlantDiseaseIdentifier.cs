namespace Smart_Farm.Application.Abstractions;

public interface IPlantDiseaseIdentifier
{
    Task<PlantDiseasePredictionResult> IdentifyAsync(
        Stream imageStream,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken);
}
