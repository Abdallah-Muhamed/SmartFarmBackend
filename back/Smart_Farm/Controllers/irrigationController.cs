using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smart_Farm.Application.Abstractions;
using Smart_Farm.DTOS;
using Smart_Farm.Infrastructure.Security;
using Smart_Farm.Models;

namespace Smart_Farm.Controllers;

/// <summary>
/// Irrigation tracking for a crop — 3 endpoints only:
///   GET  crop/{cid}           → today's recommendation (backfills missing days)
///   GET  crop/{cid}/calendar  → daily history
///   POST crop/{cid}/record    → mark irrigated / skipped
/// </summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class IrrigationController(IWaterBalanceService service, farContext db) : ControllerBase
{
    private const int EgyptUtcOffsetHours = 2;

    [HttpGet("crop/{cid:int}")]
    public async Task<ActionResult<IrrigationDayDto>> GetCropDay(
        int cid,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        var uid = UserClaims.RequireUid(User);
        var auth = CropAuthorization.EnsureCropOwnedByUser(db, cid, uid);
        if (auth is not null) return auth;

        var target = date ?? EgyptToday();

        try
        {
            var result = await service.GetDayAsync(cid, target, includeAdvice: true, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("crop/{cid:int}/calendar")]
    public async Task<ActionResult<IReadOnlyList<IrrigationCalendarDayDto>>> GetCalendar(
        int cid,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var uid = UserClaims.RequireUid(User);
        var auth = CropAuthorization.EnsureCropOwnedByUser(db, cid, uid);
        if (auth is not null) return auth;

        var end = to ?? EgyptToday();
        var start = from ?? end.AddDays(-30);

        try
        {
            var result = await service.GetCalendarAsync(cid, start, end, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("crop/{cid:int}/record")]
    public async Task<ActionResult<IrrigationDayDto>> Record(
        int cid,
        [FromBody] RecordIrrigationRequestDto body,
        CancellationToken cancellationToken)
    {
        if (body is null) return BadRequest("Request body is required.");

        var uid = UserClaims.RequireUid(User);
        var auth = CropAuthorization.EnsureCropOwnedByUser(db, cid, uid);
        if (auth is not null) return auth;

        try
        {
            var result = await service.RecordAsync(cid, body.Date, body.Applied, body.Liters, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static DateOnly EgyptToday() =>
        DateOnly.FromDateTime(DateTime.UtcNow.AddHours(EgyptUtcOffsetHours));
}
