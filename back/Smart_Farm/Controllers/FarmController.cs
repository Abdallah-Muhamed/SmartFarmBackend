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
public class FarmController : ControllerBase
{
    private readonly farContext _db;

    public FarmController(farContext db)
    {
        _db = db;
    }

    private static FarmResponseDto ToDto(FARM f, int cropCount) => new()
    {
        FarmId = f.FarmId,
        Name = f.Name,
        Latitude = f.Latitude,
        Longitude = f.Longitude,
        Governorate = f.Governorate,
        City = f.City,
        Address_line = f.Address_line,
        Area_size = f.Area_size,
        Default_Soil_type = f.Default_Soil_type,
        Notes = f.Notes,
        CreatedAt = f.CreatedAt,
        Uid = f.Uid,
        CropCount = cropCount
    };

    /// <summary>List all farms owned by the current user.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<FarmResponseDto>>> GetMine(CancellationToken ct)
    {
        var uid = UserClaims.RequireUid(User);

        var rows = await _db.FARMs
            .Where(f => f.Uid == uid)
            .Select(f => new
            {
                Farm = f,
                CropCount = _db.CROPs.Count(c => c.FarmId == f.FarmId)
            })
            .ToListAsync(ct);

        return Ok(rows.Select(r => ToDto(r.Farm, r.CropCount)));
    }

    /// <summary>Get a single farm by id (must be owned by the caller).</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FarmResponseDto>> GetById(int id, CancellationToken ct)
    {
        var uid = UserClaims.RequireUid(User);
        var farm = await _db.FARMs.FirstOrDefaultAsync(f => f.FarmId == id, ct);
        if (farm is null) return NotFound();
        if (farm.Uid != uid) return Forbid();

        var count = await _db.CROPs.CountAsync(c => c.FarmId == id, ct);
        return Ok(ToDto(farm, count));
    }

    /// <summary>List crops belonging to a farm.</summary>
    [HttpGet("{id:int}/crops")]
    public async Task<IActionResult> GetCrops(int id, CancellationToken ct)
    {
        var uid = UserClaims.RequireUid(User);
        var farm = await _db.FARMs.FirstOrDefaultAsync(f => f.FarmId == id, ct);
        if (farm is null) return NotFound();
        if (farm.Uid != uid) return Forbid();

        var crops = await _db.CROPs
            .Where(c => c.FarmId == id)
            .Select(c => new
            {
                c.Cid,
                c.Pid,
                PlantName = c.PidNavigation != null ? c.PidNavigation.Name : null,
                c.Area_size,
                c.Start_date,
                c.Soil_type,
                c.Current_Stage
            })
            .ToListAsync(ct);

        return Ok(crops);
    }

    /// <summary>Create a new farm for the current user.</summary>
    [HttpPost]
    public async Task<ActionResult<FarmResponseDto>> Create([FromBody] CreateFarmDto dto, CancellationToken ct)
    {
        if (dto is null) return BadRequest("Body is required.");
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required.");

        var uid = UserClaims.RequireUid(User);

        var farm = new FARM
        {
            Name = dto.Name.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Governorate = dto.Governorate,
            City = dto.City,
            Address_line = dto.Address_line,
            Area_size = dto.Area_size,
            Default_Soil_type = dto.Default_Soil_type,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            Uid = uid
        };

        _db.FARMs.Add(farm);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = farm.FarmId }, ToDto(farm, 0));
    }

    /// <summary>Update an owned farm.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FarmResponseDto>> Update(int id, [FromBody] UpdateFarmDto dto, CancellationToken ct)
    {
        if (dto is null) return BadRequest("Body is required.");

        var uid = UserClaims.RequireUid(User);
        var farm = await _db.FARMs.FirstOrDefaultAsync(f => f.FarmId == id, ct);
        if (farm is null) return NotFound();
        if (farm.Uid != uid) return Forbid();

        if (!string.IsNullOrWhiteSpace(dto.Name))
            farm.Name = dto.Name.Trim();

        farm.Latitude = dto.Latitude ?? farm.Latitude;
        farm.Longitude = dto.Longitude ?? farm.Longitude;
        farm.Governorate = dto.Governorate ?? farm.Governorate;
        farm.City = dto.City ?? farm.City;
        farm.Address_line = dto.Address_line ?? farm.Address_line;
        farm.Area_size = dto.Area_size ?? farm.Area_size;
        farm.Default_Soil_type = dto.Default_Soil_type ?? farm.Default_Soil_type;
        farm.Notes = dto.Notes ?? farm.Notes;

        await _db.SaveChangesAsync(ct);

        var count = await _db.CROPs.CountAsync(c => c.FarmId == id, ct);
        return Ok(ToDto(farm, count));
    }

    /// <summary>Delete a farm. Only allowed when it has no crops.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var uid = UserClaims.RequireUid(User);
        var farm = await _db.FARMs.FirstOrDefaultAsync(f => f.FarmId == id, ct);
        if (farm is null) return NotFound();
        if (farm.Uid != uid) return Forbid();

        var hasCrops = await _db.CROPs.AnyAsync(c => c.FarmId == id, ct);
        if (hasCrops)
            return Conflict(new { error = "Cannot delete a farm that still owns crops. Move or delete the crops first." });

        _db.FARMs.Remove(farm);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id, deleted = true });
    }
}
