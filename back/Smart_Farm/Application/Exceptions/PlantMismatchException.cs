namespace Smart_Farm.Application.Exceptions;

/// <summary>Image plant does not match the crop's plant — HTTP 422.</summary>
public sealed class PlantMismatchException : Exception
{
    public PlantMismatchException(string expectedPlant, string identifiedPlant)
        : base("Plant in image does not match the selected crop.")
    {
        ExpectedPlant = expectedPlant;
        IdentifiedPlant = identifiedPlant;
    }

    public string ExpectedPlant { get; }
    public string IdentifiedPlant { get; }
}
