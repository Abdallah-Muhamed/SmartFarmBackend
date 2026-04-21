using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.Application.Abstractions;
using Smart_Farm.DTOS;

namespace Smart_Farm.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AIDiagnosesController(IAIDiagnosisService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AIDiagnosisResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await service.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DiagnoseResultDto>> Diagnose([FromForm] DiagnoseRequest request, CancellationToken cancellationToken)
    {
        if (request.Image is null)
            return BadRequest("Image is required.");

        var result = await service.DiagnoseAsync(request.Image, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AIDiagnosisResponseDto>> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var item = await service.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("{id:int}/image")]
    public async Task<IActionResult> GetImage([FromRoute] int id, [FromServices] Smart_Farm.Models.farContext db, CancellationToken cancellationToken)
    {
        var entity = await db.AI_Diagnoses.FindAsync([id], cancellationToken);
        if (entity is null) return NotFound();
        if (entity.ImageBytes is null || entity.ImageBytes.Length == 0) return NotFound();

        return File(entity.ImageBytes, entity.ImageContentType ?? "application/octet-stream", entity.ImageFileName ?? $"ai-diagnosis-{id}.bin");
    }

    [HttpGet("{id:int}/report")]
    public async Task<IActionResult> GetReport([FromRoute] int id, [FromServices] Smart_Farm.Models.farContext db, CancellationToken cancellationToken)
    {
        var entity = await db.AI_Diagnoses.AsNoTracking().FirstOrDefaultAsync(x => x.ADid == id, cancellationToken);
        if (entity is null) return NotFound();
        return Ok(new { id, report = entity.GeminiArabicReport });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateAIDiagnosisRequestDto request, CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(id, request, cancellationToken);
        return updated ? Ok() : NotFound();
    }

    [HttpPost("gemini-report")]
    public async Task<ActionResult<string>> GeminiReport([FromBody] DiagnoseResultDto request, CancellationToken cancellationToken)
    {
        var report = await service.GenerateArabicReportAsync(request, cancellationToken);
        return Ok(report);
    }
}

