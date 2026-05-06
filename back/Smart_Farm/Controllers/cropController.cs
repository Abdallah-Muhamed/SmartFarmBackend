using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Smart_Farm.DTOS;
using Smart_Farm.Infrastructure.Security;
using Smart_Farm.Models;

namespace Smart_Farm.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class cropController : ControllerBase
    {
        farContext db;

        public cropController(farContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public ActionResult GetMine()
        {
            var uid = UserClaims.RequireUid(User);

            var items = db.CROPs
                .Where(c => c.Uid == uid)
                .Select(c => new CropResponseDto
                {
                    Cid = c.Cid,
                    FarmId = c.FarmId,
                    Pid = c.Pid,
                    Notes = c.Notes,
                    Area_size = c.Area_size,
                    Start_date = c.Start_date,
                    Soil_type = c.Soil_type,
                    Current_Stage = c.Current_Stage,
                    Uid = c.Uid
                })
                .ToList();

            return Ok(items);
        }

        //get by id
        [HttpGet("{id}")]
        public ActionResult getbyid(int id)
        {
            var uid = UserClaims.RequireUid(User);
            CROP? b = db.CROPs.Find(id);

            if (b == null) return NotFound();
            if (b.Uid != uid) return Forbid();
            return Ok(new CropResponseDto
            {
                Cid = b.Cid,
                FarmId = b.FarmId,
                Pid = b.Pid,
                Notes = b.Notes,
                Area_size = b.Area_size,
                Start_date = b.Start_date,
                Soil_type = b.Soil_type,
                Current_Stage = b.Current_Stage,
                Uid = b.Uid
            });
        }

        //Edit

        //DELETE
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var uid = UserClaims.RequireUid(User);

            CROP? b = db.CROPs.Find(id);
            if (b == null) return NotFound();
            if (b.Uid != uid) return Forbid();
            db.CROPs.Remove(b);
            db.SaveChanges();
            return Ok(new { id = b.Cid, deleted = true });


        }
        // add
        [HttpPost]
        public ActionResult post(CropRequestDto b)
        {
            var uid = UserClaims.RequireUid(User);

            if (b == null) return BadRequest("Tasks is null");
            if (!ModelState.IsValid) return BadRequest();
            if (!b.Pid.HasValue) return BadRequest("Pid (plant id) is required.");
            if (!db.PLANTs.Any(p => p.Pid == b.Pid.Value)) return BadRequest("Plant not found.");
            if (!b.FarmId.HasValue) return BadRequest("FarmId is required.");
            var farm = db.FARMs.Find(b.FarmId.Value);
            if (farm is null) return BadRequest("Farm not found.");
            if (farm.Uid != uid) return Forbid();
            var entity = new CROP
            {
                Pid = b.Pid,
                FarmId = b.FarmId,
                Notes = b.Notes,
                Area_size = b.Area_size,
                Start_date = b.Start_date,
                // Inherited from the farm — user doesn't supply these on the crop.
                Soil_type = farm.Default_Soil_type,
                Current_Stage = b.Current_Stage,
                Uid = farm.Uid,
                CreatedAt = DateTime.UtcNow
            };
            db.CROPs.Add(entity);
            db.SaveChanges();
            return CreatedAtAction(nameof(getbyid), new { id = entity.Cid }, new CropResponseDto
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
        //edit
        [HttpPut("{id}")]
        public ActionResult edit(CropRequestDto b, int id)
        {
            var uid = UserClaims.RequireUid(User);

            if (b == null) return BadRequest("tasks is null");
            var entity = db.CROPs.Find(id);
            if (entity == null) return NotFound();
            if (entity.Uid != uid) return Forbid();
            if (!b.Pid.HasValue) return BadRequest("Pid (plant id) is required.");
            if (!db.PLANTs.Any(p => p.Pid == b.Pid.Value)) return BadRequest("Plant not found.");
            if (b.FarmId.HasValue && b.FarmId != entity.FarmId)
            {
                var farm = db.FARMs.Find(b.FarmId.Value);
                if (farm is null) return BadRequest("Farm not found.");
                if (farm.Uid != uid) return Forbid();
                entity.FarmId = b.FarmId;
                entity.Uid = farm.Uid;
                entity.Soil_type = farm.Default_Soil_type;
            }
            entity.Pid = b.Pid;
            entity.Notes = b.Notes;
            entity.Area_size = b.Area_size;
            entity.Start_date = b.Start_date;
            entity.Current_Stage = b.Current_Stage;
            db.SaveChanges();
            return NoContent();

        }
        [HttpGet("{cid}/products")]
        public IActionResult GetProductsByCrop(int cid)
        {
            var uid = UserClaims.RequireUid(User);
            var crop = db.CROPs.Find(cid);
            if (crop is null) return NotFound();
            if (crop.Uid != uid) return Forbid();

            var products = db.PRODUCTs
                .Where(p => p.Cid == cid)
                .ToList();

            return Ok(products);
        }
        [HttpGet("{cid}/diagnosis")]
        public IActionResult GetDiagnosis(int cid)
        {
            var uid = UserClaims.RequireUid(User);
            var crop = db.CROPs.Find(cid);
            if (crop is null) return NotFound();
            if (crop.Uid != uid) return Forbid();

            var diagnosis = db.AI_Diagnoses
                .Where(d => d.Cid == cid)
                .ToList();

            return Ok(diagnosis);
        }
        [HttpGet("{cid}/fertilizers")]
        public IActionResult GetFertilizers(int cid)
        {
            var uid = UserClaims.RequireUid(User);
            var crop = db.CROPs.Find(cid);
            if (crop is null) return NotFound();
            if (crop.Uid != uid) return Forbid();

            var fertilizers = db.CROPs
                .Where(c => c.Cid == cid)
                .SelectMany(c => c.Frs)
                .ToList();

            return Ok(fertilizers);
        }
    }
}
