using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.Application.Abstractions;
using Smart_Farm.DTOS;
using Smart_Farm.Infrastructure.Security;
using Smart_Farm.Models;
using Task = System.Threading.Tasks.Task;

namespace Smart_Farm.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class FarmController : ControllerBase
{
    private readonly farContext _db;
    private readonly ILocationGeocodingService _locationGeocodingService;

    public FarmController(farContext db, ILocationGeocodingService locationGeocodingService)
    {
        _db = db;
        _locationGeocodingService = locationGeocodingService;
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

    private async Task ApplyLocationAsync(FARM farm, CreateFarmDto dto, CancellationToken ct)
    {
        LocationLookupResult? lookup = null;

        if (!string.IsNullOrWhiteSpace(dto.LocationQuery))
        {
            lookup = await _locationGeocodingService.ForwardAsync(dto.LocationQuery!, ct);
        }
        else if (dto.Latitude.HasValue && dto.Longitude.HasValue)
        {
            lookup = await _locationGeocodingService.ReverseAsync((double)dto.Latitude.Value, (double)dto.Longitude.Value, ct);
        }

        if (lookup is not null)
        {
            farm.Latitude = (decimal)lookup.Latitude;
            farm.Longitude = (decimal)lookup.Longitude;
            farm.Governorate = lookup.Governorate ?? farm.Governorate;
            farm.City = lookup.City ?? farm.City;
            farm.Address_line = lookup.AddressLine ?? farm.Address_line;
        }

        if (dto.Latitude.HasValue)
            farm.Latitude = dto.Latitude;

        if (dto.Longitude.HasValue)
            farm.Longitude = dto.Longitude;

        if (!string.IsNullOrWhiteSpace(dto.Governorate))
            farm.Governorate = dto.Governorate.Trim();

        if (!string.IsNullOrWhiteSpace(dto.City))
            farm.City = dto.City.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Address_line))
            farm.Address_line = dto.Address_line.Trim();
    }

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
            Area_size = dto.Area_size,
            Default_Soil_type = dto.Default_Soil_type,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            Uid = uid
        };

        await ApplyLocationAsync(farm, dto, ct);

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

        farm.Area_size = dto.Area_size ?? farm.Area_size;
        farm.Default_Soil_type = dto.Default_Soil_type ?? farm.Default_Soil_type;
        farm.Notes = dto.Notes ?? farm.Notes;

        await ApplyLocationAsync(farm, dto, ct);

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
