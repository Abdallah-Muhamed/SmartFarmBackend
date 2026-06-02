using Microsoft.EntityFrameworkCore;
using Smart_Farm.Application.Abstractions;
using Smart_Farm.Common;
using Smart_Farm.DTOS;
using Smart_Farm.Models;

namespace Smart_Farm.Infrastructure.Persistence;

public class AIDiagnosisRepository(farContext db) : IAIDiagnosisRepository
{
    public async System.Threading.Tasks.Task<List<DiagnoseFullResultDto>> GetAllAsync(
        int userId, CancellationToken cancellationToken)
    {
        var rows = await db.AI_Diagnoses
            .Where(d => d.UserId == userId)
            .AsNoTracking()
            .OrderByDescending(d => d.DiagnosisDate)
            .Select(d => new DiagnosisRow(
                d.ADid,
                d.DiagnosisDate,
                d.Confidence,
                d.Did,
                d.Cid,
                d.plant_image,
                d.GrogArabicReport))
            .ToListAsync(cancellationToken);

        return rows.Select(MapToResult).ToList();
    }

    private static DiagnoseFullResultDto MapToResult(DiagnosisRow row) =>
        new()
        {
            ADid = row.ADid,
            DiagnosisDate = DateTimeUtc.Normalize(row.DiagnosisDate),
            Confidence = row.Confidence,
            Did = row.Did,
            Cid = row.Cid,
            plant_image = row.plant_image,
            Report = ReportJsonSerializer.Deserialize(row.GrogArabicReport)
        };

    private sealed record DiagnosisRow(
        int ADid,
        DateTime? DiagnosisDate,
        double? Confidence,
        int? Did,
        int? Cid,
        string? plant_image,
        string? GrogArabicReport);

    public async System.Threading.Tasks.Task<AI_Diagnosis?> FindEntityByIdAsync(
        int id, CancellationToken cancellationToken)
    {
        return await db.AI_Diagnoses.FindAsync([id], cancellationToken);
    }

    public async System.Threading.Tasks.Task<Disease?> FindDiseaseByNameAsync(
        string diseaseName, CancellationToken cancellationToken)
    {
        return await db.Diseases
            .FirstOrDefaultAsync(d => d.Name == diseaseName, cancellationToken);
    }

    public async System.Threading.Tasks.Task AddDiseaseAsync(
        Disease disease, CancellationToken cancellationToken)
    {
        await db.Diseases.AddAsync(disease, cancellationToken);
    }

    public async System.Threading.Tasks.Task<CropPlantInfo?> GetCropPlantInfoByCidAsync(
        int cid, int userId, CancellationToken cancellationToken)
    {
        return await (
            from c in db.CROPs.AsNoTracking()
            join p in db.PLANTs.AsNoTracking() on c.Pid equals p.Pid into plants
            from p in plants.DefaultIfEmpty()
            where c.Cid == cid && c.Uid == userId
            select new CropPlantInfo(c.Cid, c.Pid, c.FarmId, p != null ? p.Name : null)
        ).FirstOrDefaultAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<string?> GetPlantNameByPidAsync(
        int pid, CancellationToken cancellationToken)
    {
        return await db.PLANTs
            .AsNoTracking()
            .Where(p => p.Pid == pid)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task AddAsync(
        AI_Diagnosis diagnosis, CancellationToken cancellationToken)
    {
        await db.AI_Diagnoses.AddAsync(diagnosis, cancellationToken);
    }

    public async System.Threading.Tasks.Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await db.SaveChangesAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<int> GetDiagnosisCountForUserAsync(
        int userId, CancellationToken cancellationToken)
    {
        return await db.AI_Diagnoses
            .AsNoTracking()
            .CountAsync(d => d.UserId == userId, cancellationToken);
    }
}