using Smart_Farm.DTOS;

namespace Smart_Farm.Application.Abstractions;

public interface IAIDiagnosisService
{
    // History — filtered by current user
    Task<IReadOnlyList<DiagnoseFullResultDto>> GetAllAsync(int userId, CancellationToken cancellationToken);

    Task<DiagnoseFullResultDto?> GetByIdAsync(int id, int userId, CancellationToken cancellationToken);

    Task<DiagnoseFullResultDto> DiagnoseAsync(DiagnoseRequest request, int userId, CancellationToken cancellationToken);

    Task<GroqReportDto?> RegenerateReportAsync(int id, int userId, CancellationToken cancellationToken);

    Task<int> GetDiagnosisCountForUserAsync(int userId, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, int userId, CancellationToken cancellationToken);

    Task<int> DeleteAllAsync(int userId, CancellationToken cancellationToken);
}
