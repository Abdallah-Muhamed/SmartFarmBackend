using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.Application.Abstractions;
using Smart_Farm.Application.Exceptions;
using Smart_Farm.Common;
using Smart_Farm.DTOS;
using Smart_Farm.Infrastructure.Security;
using Smart_Farm.Models;
using System.Security.Claims;

namespace Smart_Farm.Controllers;

/// <summary>
/// AI diagnoses: read + generate reports only (no edit/delete).
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AIDiagnosesController(IAIDiagnosisService service) : ControllerBase
{
    private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png"];

    private static StatusCodeResult DiagnosisFailed() =>
        new(StatusCodes.Status422UnprocessableEntity);

    private int? GetUserId()
    {
        var claim = User.FindFirstValue("uid");
        return int.TryParse(claim, out var id) ? id : null;
    }

    // ─── GET api/AIdiagnoses ─────────────────────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DiagnoseFullResultDto>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var items = await service.GetAllAsync(userId.Value, cancellationToken);
        return Ok(items);
    }

    // ─── GET api/AIdiagnoses/stats/me ────────────────────────────────────────
    [HttpGet("stats/me")]
    public async Task<ActionResult<MyDiagnosisCountDto>> GetMyDiagnosisCount(CancellationToken cancellationToken)
    {
        if (!UserClaims.TryGetUid(User, out var userId))
            return Unauthorized();

        var count = await service.GetDiagnosisCountForUserAsync(userId, cancellationToken);
        return Ok(new MyDiagnosisCountDto { UserId = userId, DiagnosisCount = count });
    }

    // ─── GET api/AIdiagnoses/{id}/report ────────────────────────────────────
    [HttpGet("{id:int}/report")]
    public async Task<IActionResult> GetReport(
        [FromRoute] int id,
        [FromServices] farContext db,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var entity = await db.AI_Diagnoses
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ADid == id && d.UserId == userId, cancellationToken);

        if (entity is null) return NotFound();

        return Ok(new { id, report = ReportJsonSerializer.Deserialize(entity.GrogArabicReport) });
    }

    // ─── POST api/AIdiagnoses — new diagnosis + report ───────────────────────
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DiagnoseFullResultDto>> Diagnose(
        [FromForm] DiagnoseRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (request.Cid <= 0)
            return BadRequest();

        if (request.Image is null)
            return BadRequest();

        if (!AllowedImageTypes.Contains(request.Image.ContentType.ToLower()))
            return BadRequest();

        try
        {
            var result = await service.DiagnoseAsync(request, userId.Value, cancellationToken);
            return Ok(result);
        }
        catch (CropNotFoundException)
        {
            return NotFound();
        }
        catch (DiagnosisUnprocessableException)
        {
            return DiagnosisFailed();
        }
        catch (Exception)
        {
            return DiagnosisFailed();
        }
    }

    // ─── POST api/AIdiagnoses/{id}/report/regenerate ─────────────────────────
    [HttpPost("{id:int}/report/regenerate")]
    public async Task<IActionResult> RegenerateReport(
        [FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!UserClaims.TryGetUid(User, out var userId))
            return Unauthorized();

        try
        {
            var report = await service.RegenerateReportAsync(id, userId, cancellationToken);
            if (report is null)
                return NotFound();

            return Ok(new { id, report });
        }
        catch (Exception)
        {
            return DiagnosisFailed();
        }
    }
}
