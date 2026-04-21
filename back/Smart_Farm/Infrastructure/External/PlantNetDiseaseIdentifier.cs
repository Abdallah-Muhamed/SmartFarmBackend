using System.Net.Http.Headers;
using System.Text.Json;
using Smart_Farm.Application.Abstractions;

namespace Smart_Farm.Infrastructure.External;

public class PlantNetDiseaseIdentifier(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<PlantNetDiseaseIdentifier> logger) : IPlantDiseaseIdentifier
{
    public async Task<PlantDiseasePredictionResult> IdentifyAsync(
        Stream imageStream,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken)
    {
        var apiKey = configuration["PlantNet:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("PlantNet:ApiKey is not configured.");

        using var httpContent = new MultipartFormDataContent();
        var fileContent = new StreamContent(imageStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "application/octet-stream");
        httpContent.Add(fileContent, "images", fileName);

        var client = httpClientFactory.CreateClient("PlantNet");
        var url = $"v2/diseases/identify?api-key={Uri.EscapeDataString(apiKey)}";
        var response = await client.PostAsync(url, httpContent, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("PlantNet API error {Status}: {Body}", response.StatusCode, json);
            throw new InvalidOperationException($"PlantNet API error: {json}");
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
            throw new InvalidOperationException("No results returned from PlantNet.");

        var first = results[0];
        string? diseaseName = null;
        var confidence = 0d;

        if (first.TryGetProperty("description", out var descEl))
            diseaseName = descEl.GetString();
        else if (first.TryGetProperty("disease", out var diseaseEl)
            && diseaseEl.TryGetProperty("name", out var nameEl))
            diseaseName = nameEl.GetString();

        if (first.TryGetProperty("score", out var scoreEl))
            confidence = scoreEl.GetDouble();

        if (string.IsNullOrWhiteSpace(diseaseName))
            throw new InvalidOperationException("Could not extract disease name from PlantNet response.");

        return new PlantDiseasePredictionResult
        {
            DiseaseName = diseaseName,
            Confidence = confidence,
            RawResponse = json
        };
    }
}
