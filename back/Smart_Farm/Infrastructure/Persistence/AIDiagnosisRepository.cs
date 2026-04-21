using Microsoft.EntityFrameworkCore;
using Smart_Farm.Application.Abstractions;
using Smart_Farm.DTOS;
using Smart_Farm.Models;

namespace Smart_Farm.Infrastructure.Persistence;

public class AIDiagnosisRepository(farContext db) : IAIDiagnosisRepository
{
    public async global::System.Threading.Tasks.Task<List<AIDiagnosisResponseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await db.AI_Diagnoses
            .AsNoTracking()
            .Select(d => new AIDiagnosisResponseDto
            {
                ADid = d.ADid,
                DiagnosisDate = d.DiagnosisDate,
                Result = d.Result,
                Did = d.Did,
                Cid = d.Cid,
                DiseaseName = d.DidNavigation != null ? d.DidNavigation.Name : null
            })
            .ToListAsync(cancellationToken);
    }

    public async global::System.Threading.Tasks.Task<AIDiagnosisResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await db.AI_Diagnoses
            .AsNoTracking()
            .Where(d => d.ADid == id)
            .Select(d => new AIDiagnosisResponseDto
            {
                ADid = d.ADid,
                DiagnosisDate = d.DiagnosisDate,
                Result = d.Result,
                Did = d.Did,
                Cid = d.Cid,
                DiseaseName = d.DidNavigation != null ? d.DidNavigation.Name : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async global::System.Threading.Tasks.Task<AI_Diagnosis?> FindEntityByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await db.AI_Diagnoses.FindAsync([id], cancellationToken);
    }

    public async global::System.Threading.Tasks.Task<Disease?> FindDiseaseByNameAsync(string diseaseName, CancellationToken cancellationToken)
    {
        return await db.Diseases.FirstOrDefaultAsync(d => d.Name == diseaseName, cancellationToken);
    }

    public async global::System.Threading.Tasks.Task AddAsync(AI_Diagnosis diagnosis, CancellationToken cancellationToken)
    {
        await db.AI_Diagnoses.AddAsync(diagnosis, cancellationToken);
    }

    public async global::System.Threading.Tasks.Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await db.SaveChangesAsync(cancellationToken);
    }
}
