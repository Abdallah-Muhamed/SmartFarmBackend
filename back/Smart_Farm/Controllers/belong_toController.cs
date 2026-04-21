using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Smart_Farm.Models;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.DTOS;
using Smart_Farm.Infrastructure.Security;
namespace Smart_Farm.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BelongToController : ControllerBase
    {
        farContext db;

        public BelongToController(farContext context)
        {
            db = context;
        }

        [HttpGet("crop/{cid}")]
        public IActionResult GetPlantsByCrop(int cid)
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            var auth = CropAuthorization.EnsureCropOwnedByUser(db, cid, uid);
            if (auth is not null) return auth;

            var data = db.BELONG_TOs
                .Where(b => b.Cid == cid)
                .Include(b => b.PidNavigation)
                .Include(b => b.CidNavigation)
                .Select(b => new Belong_to
                {
                    Cid = b.Cid,
                    CropName = b.CidNavigation.Notes,
                    Pid = b.Pid,
                    PlantName = b.PidNavigation.Name,
                    PlantCount = b.Plant_count,
                    SowTime = b.Sow_Time,
                    HarvestTime = b.Harvest_Time
                })
                .ToList();

            return Ok(data);
        }

        [HttpPost]
        public IActionResult AddPlantToCrop([FromBody] Belong_to dto)
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            var auth = CropAuthorization.EnsureCropOwnedByUser(db, dto.Cid, uid);
            if (auth is not null) return auth;

            var entry = new BELONG_TO
            {
                Cid = dto.Cid,
                Pid = dto.Pid,
                Plant_count = dto.PlantCount,
                Sow_Time = dto.SowTime,
                Harvest_Time = dto.HarvestTime
            };

            db.BELONG_TOs.Add(entry);
            db.SaveChanges();
            return Ok(dto);
        }
        [HttpPut("{cid}")]
        public IActionResult UpdateCropPlant(int cid, [FromBody] Belong_to dto)
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            var auth = CropAuthorization.EnsureCropOwnedByUser(db, cid, uid);
            if (auth is not null) return auth;

            var entry = db.BELONG_TOs.FirstOrDefault(b => b.Cid == cid && b.Pid == dto.Pid);

            if (entry == null)
                return NotFound();

            entry.Plant_count = dto.PlantCount;
            entry.Sow_Time = dto.SowTime;
            entry.Harvest_Time = dto.HarvestTime;

            db.SaveChanges();

            return Ok(entry);
        }
        [HttpDelete("{cid}")]
        public IActionResult DeleteCropPlant(int cid)
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            var auth = CropAuthorization.EnsureCropOwnedByUser(db, cid, uid);
            if (auth is not null) return auth;

            var entry = db.BELONG_TOs.FirstOrDefault(b => b.Cid == cid);

            if (entry == null)
                return NotFound();

            db.BELONG_TOs.Remove(entry);
            db.SaveChanges();

            return NoContent();
        }

    }
}


