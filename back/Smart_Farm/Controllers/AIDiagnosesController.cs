using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.Application.Abstractions;
using Smart_Farm.DTOS;
using Smart_Farm.Models;

namespace Smart_Farm.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AIDiagnosesController(IAIDiagnosisService service, IConfiguration configuration) : ControllerBase
{
    private readonly Cloudinary _cloudinary = new(new Account(
        configuration["Cloudinary:CloudName"],
        configuration["Cloudinary:ApiKey"],
        configuration["Cloudinary:ApiSecret"]
    ));

    // ─── GET api/AIdiagnoses ─────────────────────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AIDiagnosisResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await service.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    // ─── POST api/AIdiagnoses ────────────────────────────────────────────────
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DiagnoseResultDto>> Diagnose([FromForm] DiagnoseRequest request, CancellationToken cancellationToken)
    {
        if (request.Image is null)
            return BadRequest("Image is required.");

        var result = await service.DiagnoseAsync(request.Image, cancellationToken);
        return Ok(result);
    }

    // ─── GET api/AIdiagnoses/{id} ────────────────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AIDiagnosisResponseDto>> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var item = await service.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    // ─── GET api/AIdiagnoses/{id}/image ─────────────────────────────────────
    [HttpGet("{id:int}/image")]
    public async Task<IActionResult> GetImage([FromRoute] int id, [FromServices] farContext db, CancellationToken cancellationToken)
    {
        var entity = await db.AI_Diagnoses.FindAsync([id], cancellationToken);
        if (entity is null) return NotFound();

        if (string.IsNullOrWhiteSpace(entity.plant_image))
            return NotFound("No image found.");

        return Ok(new { plant_image = entity.plant_image });
    }

    // ─── PUT api/AIdiagnoses/{id}/image ─────────────────────────────────────
    [HttpPut("{id:int}/image")] 
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage([FromRoute] int id, IFormFile file, [FromServices] farContext db, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType.ToLower()))
            return BadRequest("Only jpg, png, webp images are allowed.");

        var entity = await db.AI_Diagnoses.FindAsync([id], cancellationToken);
        if (entity is null) return NotFound();

        // Delete old image from Cloudinary if exists
        if (!string.IsNullOrWhiteSpace(entity.plant_image))
        {
            var oldPublicId = ExtractPublicId(entity.plant_image);
            if (!string.IsNullOrWhiteSpace(oldPublicId))
                await _cloudinary.DestroyAsync(new DeletionParams(oldPublicId));
        }

        // Upload new image
        await using var stream = file.OpenReadStream();
        var uploadResult = await _cloudinary.UploadAsync(new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "smart_farm/diagnoses",
            PublicId = $"diagnosis_{id}",
            Overwrite = true
        });

        if (uploadResult.Error is not null)
            return StatusCode(500, uploadResult.Error.Message);

        entity.plant_image = uploadResult.SecureUrl.ToString();
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new { plant_image = entity.plant_image });
    }

    // ─── DELETE api/AIdiagnoses/{id}/image ──────────────────────────────────
    [HttpDelete("{id:int}/image")]
    public async Task<IActionResult> DeleteImage([FromRoute] int id, [FromServices] farContext db, CancellationToken cancellationToken)
    {
        var entity = await db.AI_Diagnoses.FindAsync([id], cancellationToken);
        if (entity is null) return NotFound();

        if (string.IsNullOrWhiteSpace(entity.plant_image))
            return BadRequest("No image to delete.");

        var publicId = ExtractPublicId(entity.plant_image);
        if (!string.IsNullOrWhiteSpace(publicId))
            await _cloudinary.DestroyAsync(new DeletionParams(publicId));

        entity.plant_image = null;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new { deleted = true });
    }

    // ─── GET api/AIdiagnoses/{id}/report ────────────────────────────────────
    [HttpGet("{id:int}/report")]
    public async Task<IActionResult> GetReport([FromRoute] int id, [FromServices] farContext db, CancellationToken cancellationToken)
    {
        var entity = await db.AI_Diagnoses.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ADid == id, cancellationToken);
        if (entity is null) return NotFound();

        return Ok(new { id, report = entity.GeminiArabicReport });
    }

    // ─── PUT api/AIdiagnoses/{id} ────────────────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateAIDiagnosisRequestDto request, CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(id, request, cancellationToken);
        return updated ? Ok() : NotFound();
    }

    // ─── POST api/AIdiagnoses/gemini-report ──────────────────────────────────
    [HttpPost("gemini-report")]
    public async Task<ActionResult<string>> GeminiReport([FromBody] DiagnoseResultDto request, CancellationToken cancellationToken)
    {
        var report = await service.GenerateArabicReportAsync(request, cancellationToken);
        return Ok(report);
    }

    // ─── Helper ──────────────────────────────────────────────────────────────
    private static string? ExtractPublicId(string url)
    {
        try
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/');
            var uploadIdx = Array.IndexOf(segments, "upload");
            if (uploadIdx < 0) return null;
            var start = uploadIdx + 1;
            if (start < segments.Length && segments[start].StartsWith('v') &&
                long.TryParse(segments[start][1..], out _))
                start++;
            var publicIdWithExt = string.Join("/", segments[start..]);
            var dot = publicIdWithExt.LastIndexOf('.');
            return dot >= 0 ? publicIdWithExt[..dot] : publicIdWithExt;
        }
        catch { return null; }
    }
}