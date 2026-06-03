using System.Net.Http.Headers;
using System.Text.Json;
using Smart_Farm.Application.Abstractions;
using Smart_Farm.Application.Exceptions;

namespace Smart_Farm.Infrastructure.External;

public class PlantNetSpeciesIdentifier(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<PlantNetSpeciesIdentifier> logger) : IPlantSpeciesIdentifier
{
    public async Task<PlantSpeciesIdentificationResult> IdentifyAsync(
        Stream imageStream,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken)
    {
        var apiKey = configuration["PlantNet:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("PlantNet:ApiKey is not configured.");

        var project = configuration["PlantNet:IdentificationProject"] ?? "all";
        var lang = configuration["PlantNet:Language"] ?? "ar";

        using var httpContent = new MultipartFormDataContent();
        var fileContent = new StreamContent(imageStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "image/jpeg");
        httpContent.Add(fileContent, "images", fileName);
        httpContent.Add(new StringContent("auto"), "organs");

        var client = httpClientFactory.CreateClient("PlantNet");
        var url =
            $"v2/identify/{Uri.EscapeDataString(project)}?api-key={Uri.EscapeDataString(apiKey)}&lang={Uri.EscapeDataString(lang)}&nb-results=5";

        var response = await client.PostAsync(url, httpContent, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("PlantNet species API error {Status}: {Body}", response.StatusCode, json);
            throw new DiagnosisUnprocessableException(
                new InvalidOperationException($"PlantNet species API error: {json}"));
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
            throw new DiagnosisUnprocessableException(
                new InvalidOperationException("No species results returned from PlantNet."));

        var first = results[0];
        var confidence = first.TryGetProperty("score", out var scoreEl) ? scoreEl.GetDouble() : 0d;

        var bestMatch = root.TryGetProperty("bestMatch", out var bestEl)
            ? bestEl.GetString() ?? string.Empty
            : string.Empty;

        string? scientificName = null;
        var commonNames = new List<string>();

        if (first.TryGetProperty("species", out var species))
        {
            if (species.TryGetProperty("scientificNameWithoutAuthor", out var sciEl))
                scientificName = sciEl.GetString();
            else if (species.TryGetProperty("scientificName", out var sciFullEl))
                scientificName = sciFullEl.GetString();

            if (species.TryGetProperty("commonNames", out var namesEl)
                && namesEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in namesEl.EnumerateArray())
                {
                    var name = item.GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                        commonNames.Add(name.Trim());
                }
            }
        }

        if (string.IsNullOrWhiteSpace(bestMatch) && !string.IsNullOrWhiteSpace(scientificName))
            bestMatch = scientificName;

        if (string.IsNullOrWhiteSpace(bestMatch))
            throw new DiagnosisUnprocessableException(
                new InvalidOperationException("Could not extract plant name from PlantNet response."));

        return new PlantSpeciesIdentificationResult
        {
            BestMatch = bestMatch,
            ScientificName = scientificName,
            ArabicName = commonNames.FirstOrDefault(),
            CommonNames = commonNames,
            Confidence = confidence,
            RawResponse = json
        };
    }
}
