using Smart_Farm.DTOS;
using Smart_Farm.Models;

namespace Smart_Farm.Application.Abstractions;

public interface IAIDiagnosisRepository
{
    // Filtered by user — for history endpoint
    System.Threading.Tasks.Task<List<DiagnoseFullResultDto>> GetAllAsync(int userId, CancellationToken cancellationToken);

    System.Threading.Tasks.Task<DiagnoseFullResultDto?> GetByIdAsync(
        int id, int userId, CancellationToken cancellationToken);

    System.Threading.Tasks.Task<AI_Diagnosis?> FindEntityByIdAsync(int id, CancellationToken cancellationToken);

    System.Threading.Tasks.Task<Disease?> FindDiseaseByNameAsync(string diseaseName, CancellationToken cancellationToken);

    // Creates a new disease row when AI returns an unknown name
    System.Threading.Tasks.Task AddDiseaseAsync(Disease disease, CancellationToken cancellationToken);

    // Cid → Pid → PLANT.Name (crop must belong to user)
    System.Threading.Tasks.Task<CropPlantInfo?> GetCropPlantInfoByCidAsync(
        int cid, int userId, CancellationToken cancellationToken);

    System.Threading.Tasks.Task<string?> GetPlantNameByPidAsync(
        int pid, CancellationToken cancellationToken);

    System.Threading.Tasks.Task AddAsync(AI_Diagnosis diagnosis, CancellationToken cancellationToken);
    System.Threading.Tasks.Task SaveChangesAsync(CancellationToken cancellationToken);

    System.Threading.Tasks.Task<int> GetDiagnosisCountForUserAsync(
        int userId, CancellationToken cancellationToken);

    System.Threading.Tasks.Task<List<string?>> GetImageUrlsForUserAsync(
        int userId, CancellationToken cancellationToken);

    System.Threading.Tasks.Task<bool> DeleteForUserAsync(
        int id, int userId, CancellationToken cancellationToken);

    System.Threading.Tasks.Task<int> DeleteAllForUserAsync(
        int userId, CancellationToken cancellationToken);
}
