using Smart_Farm.DTOS;
using Smart_Farm.Models;

namespace Smart_Farm.Application.Abstractions;

public interface IAIDiagnosisRepository
{
    global::System.Threading.Tasks.Task<List<AIDiagnosisResponseDto>> GetAllAsync(CancellationToken cancellationToken);
    global::System.Threading.Tasks.Task<AIDiagnosisResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    global::System.Threading.Tasks.Task<AI_Diagnosis?> FindEntityByIdAsync(int id, CancellationToken cancellationToken);
    global::System.Threading.Tasks.Task<Disease?> FindDiseaseByNameAsync(string diseaseName, CancellationToken cancellationToken);
    global::System.Threading.Tasks.Task AddAsync(AI_Diagnosis diagnosis, CancellationToken cancellationToken);
    global::System.Threading.Tasks.Task SaveChangesAsync(CancellationToken cancellationToken);
}
