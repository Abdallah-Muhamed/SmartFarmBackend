using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.DTOS;
using Smart_Farm.Infrastructure.Persistence;
using Smart_Farm.Infrastructure.Security;
using Smart_Farm.Models;

namespace Smart_Farm.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CropController(farContext db) : ControllerBase
{
    private readonly farContext _db = db;

    // ????????????? GET mine ?????????????
    [HttpGet]
    public async Task<ActionResult> GetMine()
    {
        var uid = UserClaims.RequireUid(User);

        var items = await _db.CROPs
            .AsNoTracking()
            .Where(c => c.Uid == uid)
            .Select(c => new CropResponseDto
            {
                Cid = c.Cid,
                FarmId = c.FarmId,
                Pid = c.Pid,
                PlantName = c.PidNavigation != null ? c.PidNavigation.Name : null,
                FarmName = c.FarmNavigation != null ? c.FarmNavigation.Name : null,
                PhotoUrl = c.PidNavigation != null ? c.PidNavigation.PhotoUrl : null,
                Notes = c.Notes,
                Area_size = c.Area_size,
                Start_date = c.Start_date,
                Soil_type = c.Soil_type,
                Current_Stage = c.Current_Stage,
                Uid = c.Uid
            })
            .ToListAsync();

        return Ok(items);
    }

    // ????????????? GET by id ?????????????
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        var uid = UserClaims.RequireUid(User);

        var crop = await _db.CROPs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Cid == id);

        if (crop is null)
            return NotFound();

        if (crop.Uid != uid)
            return Forbid();

        return Ok(new CropResponseDto
        {
            Cid = crop.Cid,
            FarmId = crop.FarmId,
            Pid = crop.Pid,
            Notes = crop.Notes,
            Area_size = crop.Area_size,
            Start_date = crop.Start_date,
            Soil_type = crop.Soil_type,
            Current_Stage = crop.Current_Stage,
            Uid = crop.Uid
        });
    }

    // ????????????? POST ?????????????
    [HttpPost]
    public async Task<ActionResult> Create(CropRequestDto dto, CancellationToken ct)
    {
        var uid = UserClaims.RequireUid(User);

        if (dto is null)
            return BadRequest("Crop is null");

        if (!dto.Pid.HasValue)
            return BadRequest("Pid is required");

        if (!dto.FarmId.HasValue)
            return BadRequest("FarmId is required");

        var plantExists = await _db.PLANTs.AnyAsync(p => p.Pid == dto.Pid.Value, ct);
        if (!plantExists)
            return BadRequest("Plant not found");

        var farm = await _db.FARMs.FirstOrDefaultAsync(f => f.FarmId == dto.FarmId.Value, ct);
        if (farm is null)
            return BadRequest("Farm not found");

        if (farm.Uid != uid)
            return Forbid();

        var entity = new CROP
        {
            Pid = dto.Pid,
            FarmId = dto.FarmId,
            Notes = dto.Notes,
            Area_size = dto.Area_size,
            Start_date = dto.Start_date,
            Soil_type = farm.Default_Soil_type,
            Current_Stage = dto.Current_Stage,
            Uid = uid,
            CreatedAt = DateTime.UtcNow
        };

        _db.CROPs.Add(entity);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entity.Cid }, new CropResponseDto
        {
            Cid = entity.Cid,
            FarmId = entity.FarmId,
            Pid = entity.Pid,
            Notes = entity.Notes,
            Area_size = entity.Area_size,
            Start_date = entity.Start_date,
            Soil_type = entity.Soil_type,
            Current_Stage = entity.Current_Stage,
            Uid = entity.Uid
        });
    }

    // ????????????? PUT ?????????????
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, CropRequestDto dto, CancellationToken ct)
    {
        var uid = UserClaims.RequireUid(User);

        var entity = await _db.CROPs.FirstOrDefaultAsync(c => c.Cid == id, ct);

        if (entity is null)
            return NotFound();

        if (entity.Uid != uid)
            return Forbid();

        if (!dto.Pid.HasValue)
            return BadRequest("Pid is required");

        var plantExists = await _db.PLANTs.AnyAsync(p => p.Pid == dto.Pid.Value, ct);
        if (!plantExists)
            return BadRequest("Plant not found");

        if (dto.FarmId.HasValue && dto.FarmId != entity.FarmId)
        {
            var farm = await _db.FARMs.FirstOrDefaultAsync(f => f.FarmId == dto.FarmId.Value, ct);

            if (farm is null)
                return BadRequest("Farm not found");

            if (farm.Uid != uid)
                return Forbid();

            entity.FarmId = dto.FarmId;
            entity.Soil_type = farm.Default_Soil_type;
            entity.Uid = uid;
        }

        entity.Pid = dto.Pid;
        entity.Notes = dto.Notes;
        entity.Area_size = dto.Area_size;
        entity.Start_date = dto.Start_date;
        entity.Current_Stage = dto.Current_Stage;

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    /// <summary>Delete all crops in a farm owned by the current user.</summary>
    [HttpDelete("all")]
    public async Task<ActionResult> DeleteAll([FromQuery] int farmId, CancellationToken ct)
    {
        if (farmId <= 0)
            return BadRequest("farmId is required.");

        var uid = UserClaims.RequireUid(User);

        var farm = await _db.FARMs.FirstOrDefaultAsync(f => f.FarmId == farmId, ct);
        if (farm is null)
            return NotFound();

        if (farm.Uid != uid)
            return Forbid();

        var cropIds = await _db.CROPs
            .Where(c => c.Uid == uid && c.FarmId == farmId)
            .Select(c => c.Cid)
            .ToListAsync(ct);

        await CropDeletionHelper.RemoveDependenciesAsync(_db, cropIds, ct);

        var deletedCount = await _db.CROPs
            .Where(c => c.Uid == uid && c.FarmId == farmId)
            .ExecuteDeleteAsync(ct);

        return Ok(new { farmId, deletedCount });
    }

    // ????????????? DELETE ?????????????
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        var uid = UserClaims.RequireUid(User);

        var entity = await _db.CROPs.FirstOrDefaultAsync(c => c.Cid == id, ct);

        if (entity is null)
            return NotFound();

        if (entity.Uid != uid)
            return Forbid();

        await CropDeletionHelper.RemoveDependenciesAsync(_db, [entity.Cid], ct);

        _db.CROPs.Remove(entity);
        await _db.SaveChangesAsync(ct);

        return Ok(new { id, deleted = true });
    }

    // ????????????? Crop AI / diagnosis ?????????????
    [HttpGet("{cid:int}/diagnosis")]
    public async Task<ActionResult> GetDiagnosis(int cid)
    {
        var uid = UserClaims.RequireUid(User);

        var crop = await _db.CROPs.FirstOrDefaultAsync(c => c.Cid == cid);

        if (crop is null)
            return NotFound();

        if (crop.Uid != uid)
            return Forbid();

        var diagnosis = await _db.AI_Diagnoses
            .AsNoTracking()
            .Where(d => d.Cid == cid)
            .ToListAsync();

        return Ok(diagnosis);
    }

    // ????????????? Fertilizers (FIXED) ?????????????
    [HttpGet("{cid:int}/fertilizers")]
    public async Task<ActionResult> GetFertilizers(int cid)
    {
        var uid = UserClaims.RequireUid(User);

        var crop = await _db.CROPs
            .Include(c => c.Frs)
            .FirstOrDefaultAsync(c => c.Cid == cid);

        if (crop is null)
            return NotFound();

        if (crop.Uid != uid)
            return Forbid();

        return Ok(crop.Frs);
    }
}