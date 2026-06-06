using Smart_Farm.DTOS;

namespace Smart_Farm.Application.Abstractions;

public interface IWaterBalanceService
{
    /// <summary>Backfills missing days through <paramref name="date"/> and returns that day's status.</summary>
    Task<IrrigationDayDto> GetDayAsync(int cid, DateOnly date, bool includeAdvice, CancellationToken cancellationToken);

    /// <summary>Daily irrigation calendar for a crop (backfills through <paramref name="to"/>).</summary>
    Task<IReadOnlyList<IrrigationCalendarDayDto>> GetCalendarAsync(
        int cid, DateOnly from, DateOnly to, CancellationToken cancellationToken);

    /// <summary>Record whether the farmer irrigated on a given day.</summary>
    Task<IrrigationDayDto> RecordAsync(
        int cid, DateOnly date, bool applied, decimal? appliedLiters, CancellationToken cancellationToken);

    /// <summary>Used by the morning background job to persist today's balance for all crops.</summary>
    Task<IrrigationDayDto> SyncDayAsync(int cid, DateOnly date, CancellationToken cancellationToken);
}
