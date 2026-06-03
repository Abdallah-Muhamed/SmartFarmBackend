using System.Text;
using System.Text.Json;
using Smart_Farm.Application.Abstractions;
using Smart_Farm.Common;
using Smart_Farm.DTOS;

namespace Smart_Farm.Infrastructure.External;

public class GrogIrrigationAdviceGenerator(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<GrogIrrigationAdviceGenerator> logger) : IIrrigationAdviceGenerator
{
    public async Task<IrrigationAdviceReportDto> GenerateArabicAdviceAsync(
        IrrigationAdviceInput input,
        CancellationToken cancellationToken)
    {
        var apiKey = configuration["Groq:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Groq:ApiKey is not configured.");

        var model = configuration["Groq:Model"] ?? "meta-llama/llama-4-scout-17b-16e-instruct";
        var endpoint = "https://api.groq.com/openai/v1/chat/completions";

        var contextJson = ReportJsonSerializer.SerializeObject(BuildContext(input));
        var prompt = BuildPrompt(contextJson);

        var payload = JsonSerializer.Serialize(new
        {
            model,
            messages = new[] { new { role = "user", content = prompt } },
            temperature = 0.35
        });

        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(endpoint, content, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Groq irrigation advice failed {StatusCode}: {Body}", response.StatusCode, responseBody);
            throw new InvalidOperationException($"Groq request failed: {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Groq returned an empty response.");

        var clean = text.Trim();
        if (clean.StartsWith("```"))
        {
            clean = clean.Split('\n', 2).Last();
            clean = clean[..clean.LastIndexOf("```")];
        }

        using var resultDoc = JsonDocument.Parse(clean.Trim());
        var root = resultDoc.RootElement;

        return new IrrigationAdviceReportDto
        {
            Summary = GetString(root, "summary"),
            Recommendation = GetString(root, "recommendation"),
            Timing = GetString(root, "timing"),
            ApplicationTips = GetString(root, "applicationTips"),
            SoilWeatherNotes = GetString(root, "soilWeatherNotes"),
            Warnings = GetString(root, "warnings")
        };
    }

    private static string GetString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var el) ? el.GetString() ?? string.Empty : string.Empty;

    private static object BuildContext(IrrigationAdviceInput input)
    {
        var r = input.Recommendation;
        return new
        {
            r.Cid,
            date = r.Date.ToString("yyyy-MM-dd"),
            r.PlantName,
            r.StageName,
            r.SoilType,
            r.AreaFeddan,
            r.IsIrrigationDay,
            r.Recommended_m3_per_feddan,
            r.Recommended_m3_field,
            r.Recommended_Liters_field,
            r.ET0_mm,
            r.Kc,
            r.ETc_mm,
            r.EffRain_mm,
            r.TAW_mm,
            r.RAW_mm,
            r.DeplStart_mm,
            r.DeplAfterEt_mm,
            r.DeplEnd_mm,
            r.Irrig_mm,
            weather = new
            {
                rain_mm = input.Rain_mm,
                tmin_C = input.Tmin_C,
                tmax_C = input.Tmax_C
            },
            notes = r.IsIrrigationDay
                ? "اليوم يوم ري حسب نموذج الميزان المائي — الكميات الموصى بها محسوبة مسبقاً ولا تغيّرها."
                : "اليوم ليس يوم ري حسب نموذج الميزان المائي — لا تطلب كميات إضافية عن الصفر."
        };
    }

    private static string BuildPrompt(string contextJson)
    {
        return $$"""
أنت مهندس زراعي وخبير ري في مصر. اقرأ بيانات الميزان المائي والطقس التالية (محسوبة من النظام) واكتب نصائح عملية للمزارع لليوم المحدد.

البيانات:
{{contextJson}}

تعليمات صارمة:
- الرد بالكامل بالعربية الفصحى المبسطة — ممنوع أي كلمة إنجليزية أو رموز لاتينية في النصوص
- لا تغيّر قرار isIrrigationDay ولا الكميات الموصى بها (recommended_m3_per_feddan، recommended_m3_field، recommended_Liters_field) — اشرحها وفسّرها فقط
- إذا isIrrigationDay = false: أوضح أن الري غير مطلوب اليوم ومتى يُتوقع الحاجة التالية إن أمكن
- إذا isIrrigationDay = true: اذكر الكمية بالمتر المكعب للحقل واللتر تقريباً، ونصائح التوزيع (تنقيط/رش/غمر حسب نوع التربة)
- اربط النصائح بمرحلة النمو (stageName) ونوع التربة (soilType)
- استخدم أرقام البيانات كما هي عند الحاجة (مم، م³، فدان)
- كل حقل لا يزيد عن 4 أسطر

رد فقط بـ JSON صالح بهذا الشكل بالضبط بدون markdown أو نص إضافي:
{
  "summary": "ملخص وضع الماء في الحقل اليوم",
  "recommendation": "هل تروي اليوم وكم بالتفصيل للمزارع",
  "timing": "أفضل وقت للري اليوم أو غداً إن لزم",
  "applicationTips": "طريقة التطبيق وتقسيم الجلسات",
  "soilWeatherNotes": "تأثير التربة والأمطار ودرجات الحرارة على القرار",
  "warnings": "تحذيرات (إجهاد مائي، ري زائد، أمطار متوقعة) أو «لا توجد تحذيرات خاصة»"
}
""";
    }
}
