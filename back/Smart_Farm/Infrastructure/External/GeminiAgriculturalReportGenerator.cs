using System.Text;
using System.Text.Json;
using Smart_Farm.Application.Abstractions;
using Smart_Farm.DTOS;

namespace Smart_Farm.Infrastructure.External;

public class GeminiAgriculturalReportGenerator(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<GeminiAgriculturalReportGenerator> logger) : IAgriculturalReportGenerator
{
    public async Task<string> GenerateArabicReportAsync(DiagnoseResultDto diagnoseResult, CancellationToken cancellationToken)
    {
        var apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Gemini:ApiKey is not configured.");

        var model = configuration["Gemini:Model"] ?? "gemini-2.0-flash";
        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";

        var apiResponseJson = JsonSerializer.Serialize(new
        {
            id = diagnoseResult.Id,
            disease = diagnoseResult.Disease,
            confidence = diagnoseResult.Confidence,
            saved = diagnoseResult.Saved
        });

        var prompt = BuildPrompt(apiResponseJson);

        var payload = JsonSerializer.Serialize(new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        });

        var client = httpClientFactory.CreateClient();
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(endpoint, content, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Gemini request failed with status {StatusCode}: {Body}", response.StatusCode, responseBody);
            throw new InvalidOperationException($"Gemini request failed: {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        var text = root
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Gemini returned an empty response.");

        return text;
    }

    private static string BuildPrompt(string apiResponseJson)
    {
        return $"""
You are an agricultural expert specializing in plant disease diagnosis.

You will receive a JSON response from the PlantNet API. This response contains plant identification results along with confidence scores.

Your task is to:
Analyze the JSON response carefully.
Focus on the top results with the highest confidence scores.
Consider the confidence level when making your decision:
If confidence is high -> provide a clear and confident diagnosis.
If confidence is medium -> mention possible alternatives.
If confidence is low -> clearly state uncertainty and provide the most likely possibilities.

Then generate a FULL detailed report in ARABIC ONLY.

The report must be well-structured and contain the following sections:

التشخيص:
حدد المرض أو النبات المحتمل بناءً على أعلى نسبة ثقة، واذكر نسبة الثقة بشكل واضح.

الاعراض:
اشرح الأعراض المرتبطة بهذا المرض أو النبات بطريقة مبسطة وواضحة.

العلاج:
اقترح طرق علاج عملية (مبيدات، طرق طبيعية، تحسين الري، إلخ).

الوقاية:
قدم نصائح لمنع حدوث المشكلة مستقبلاً.

Important rules:
The entire response MUST be in Arabic.
Use simple, clear Arabic (مفهومة للمزارعين).
Do NOT output JSON.
Do NOT mention that you are an AI.
Be practical and actionable.

Here is the PlantNet API response:
{apiResponseJson}
""";
    }
}
