using Smart_Farm.Application.Abstractions;
using Smart_Farm.DTOS;
using Smart_Farm.Models;

namespace Smart_Farm.Application.Services;

public class AIDiagnosisService(
    IAIDiagnosisRepository repository,
    IPlantDiseaseIdentifier plantDiseaseIdentifier,
    IAgriculturalReportGenerator reportGenerator) : IAIDiagnosisService
{
    public async Task<IReadOnlyList<AIDiagnosisResponseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(cancellationToken);
    }

    public async Task<AIDiagnosisResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<DiagnoseResultDto> DiagnoseAsync(IFormFile image, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
            throw new ArgumentException("No image uploaded", nameof(image));

        byte[] bytes;
        await using (var ms = new MemoryStream())
        {
            await image.CopyToAsync(ms, cancellationToken);
            bytes = ms.ToArray();
        }

        await using var stream = new MemoryStream(bytes);
        var prediction = await plantDiseaseIdentifier.IdentifyAsync(
            stream,
            image.FileName,
            image.ContentType,
            cancellationToken);

        var disease = await repository.FindDiseaseByNameAsync(prediction.DiseaseName, cancellationToken);

        var diagnosis = new AI_Diagnosis
        {
            DiagnosisDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Result = prediction.DiseaseName,
            Did = disease?.Did,
            ImageBytes = bytes,
            ImageContentType = image.ContentType,
            ImageFileName = image.FileName
        };

        await repository.AddAsync(diagnosis, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return new DiagnoseResultDto
        {
            Id = diagnosis.ADid,
            Disease = prediction.DiseaseName,
            Confidence = prediction.Confidence,
            Saved = true
        };
    }

    public async Task<string> GenerateArabicReportAsync(DiagnoseResultDto diagnoseResult, CancellationToken cancellationToken)
    {
        var report = await reportGenerator.GenerateArabicReportAsync(diagnoseResult, cancellationToken);

        // Persist report against the diagnosis id when possible
        var entity = await repository.FindEntityByIdAsync(diagnoseResult.Id, cancellationToken);
        if (entity is not null)
        {
            entity.GeminiArabicReport = report;
            await repository.SaveChangesAsync(cancellationToken);
        }

        return report;
    }

    public async Task<bool> UpdateAsync(int id, UpdateAIDiagnosisRequestDto request, CancellationToken cancellationToken)
    {
        var existing = await repository.FindEntityByIdAsync(id, cancellationToken);
        if (existing is null)
            return false;

        existing.DiagnosisDate = request.DiagnosisDate;
        existing.Result = request.Result;
        existing.Did = request.Did;
        existing.Cid = request.Cid;

        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
