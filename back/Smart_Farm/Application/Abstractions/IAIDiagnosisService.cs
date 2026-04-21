using Microsoft.AspNetCore.Http;
using Smart_Farm.DTOS;

namespace Smart_Farm.Application.Abstractions;

public interface IAIDiagnosisService
{
    Task<IReadOnlyList<AIDiagnosisResponseDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<AIDiagnosisResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<DiagnoseResultDto> DiagnoseAsync(IFormFile image, CancellationToken cancellationToken);
    Task<string> GenerateArabicReportAsync(DiagnoseResultDto diagnoseResult, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(int id, UpdateAIDiagnosisRequestDto request, CancellationToken cancellationToken);
}
