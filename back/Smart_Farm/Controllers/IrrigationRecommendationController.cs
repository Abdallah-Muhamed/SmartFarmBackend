using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smart_Farm.Application.Abstractions;

namespace Smart_Farm.Controllers;

[Authorize]
[Route("api/irrigation-recommendation")]
[ApiController]
public class IrrigationRecommendationController : ControllerBase
{
    private readonly IWaterBalanceService _service;

    public IrrigationRecommendationController(IWaterBalanceService service)
    {
        _service = service;
    }

    /// <summary>
    /// Returns today's (or a given date's) irrigation recommendation for a crop.
    /// </summary>
    /// <param name="cid">Crop id.</param>
    /// <param name="date">Optional YYYY-MM-DD. Defaults to today (UTC).</param>
    /// <param name="persist">If true, updates CROP.Depletion_mm and writes a balance log entry. Default: false (dry-run).</param>
    [HttpGet("{cid:int}")]
    public async Task<ActionResult<IrrigationRecommendationDto>> Get(
        int cid,
        [FromQuery] DateOnly? date,
        [FromQuery] bool persist = false,
        CancellationToken cancellationToken = default)
    {
        var target = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        try
        {
            var result = await _service.ComputeDailyAsync(cid, target, persist, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
