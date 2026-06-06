using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.DTOS;
using Smart_Farm.Infrastructure.Security;
using Smart_Farm.Models;

namespace Smart_Farm.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PlantStageController(farContext context) : ControllerBase
{
    private readonly farContext db = context;

    [HttpGet("crop/{cid}")]
    public async Task<IActionResult> GetByCrop(int cid, CancellationToken cancellationToken)
    {
        var uid = UserClaims.RequireUid(User);

        var auth = CropAuthorization.EnsureCropOwnedByUser(db, cid, uid);
        if (auth is not null) return auth;

        var crop = await db.CROPs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Cid == cid, cancellationToken);

        if (crop is null) return NotFound();

        if (crop.Pid is null)
            return Ok(Array.Empty<PlantStageDto>());

        var stages = await db.PLANT_STAGEs
            .AsNoTracking()
            .Where(ps => ps.Pid == crop.Pid)
            .OrderBy(ps => ps.Stage_order)
            .Select(ps => new PlantStageDto
            {
                PSid = ps.PSid,
                Pid = ps.Pid,
                Name_stage = ps.Name_stage,
                Stage_order = ps.Stage_order,
                Duration_days = ps.Duration_days,
                Description = ps.Description
            })
            .ToListAsync(cancellationToken);

        return Ok(stages);
    }
}
