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

        //list
        [HttpGet]
        public ActionResult getall()
        {
            var items = db.CROPs.Select(c => new CropResponseDto
            {
                Cid = c.Cid,
                Notes = c.Notes,
                Area_size = c.Area_size,
                Start_date = c.Start_date,
                Soil_type = c.Soil_type,
                Current_Stage = c.Current_Stage,
                Uid = c.Uid
            }).ToList();
            return Ok(items);
        }

        [HttpGet("me")]
        public ActionResult GetMine()
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            var items = db.CROPs
                .Where(c => c.Uid == uid)
                .Select(c => new CropResponseDto
                {
                    Cid = c.Cid,
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
            CROP? b = db.CROPs.Find(id);

            if (b == null) return NotFound();
            return Ok(new CropResponseDto
            {
                Cid = b.Cid,
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
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

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
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            if (b == null) return BadRequest("Tasks is null");
            if (!ModelState.IsValid) return BadRequest();
            var entity = new CROP
            {
                Notes = b.Notes,
                Area_size = b.Area_size,
                Start_date = b.Start_date,
                Soil_type = b.Soil_type,
                Current_Stage = b.Current_Stage,
                Uid = uid
            };
            db.CROPs.Add(entity);
            db.SaveChanges();
            return CreatedAtAction(nameof(getbyid), new { id = entity.Cid }, new CropResponseDto
            {
                Cid = entity.Cid,
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
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            if (b == null) return BadRequest("tasks is null");
            var entity = db.CROPs.Find(id);
            if (entity == null) return NotFound();
            if (entity.Uid != uid) return Forbid();
            entity.Notes = b.Notes;
            entity.Area_size = b.Area_size;
            entity.Start_date = b.Start_date;
            entity.Soil_type = b.Soil_type;
            entity.Current_Stage = b.Current_Stage;
            db.SaveChanges();
            return NoContent();

        }
        [HttpGet("{cid}/products")]
        public IActionResult GetProductsByCrop(int cid)
        {
            var products = db.PRODUCTs
                .Where(p => p.Cid == cid)
                .ToList();

            return Ok(products);
        }
        [HttpGet("{cid}/irrigation-stages")]
        public IActionResult GetIrrigationStages(int cid)
        {
            var stages = db.IRRIGATION_STAGEs
                .Where(i => i.Cid == cid)
                .ToList();

            return Ok(stages);
        }
        [HttpGet("{cid}/irrigations")]
        public IActionResult GetIrrigations(int cid)
        {
            var irrigations = db.IRRIGATIONs
                .Where(i => i.Cid == cid)
                .ToList();

            return Ok(irrigations);
        }
        [HttpGet("{cid}/diagnosis")]
        public IActionResult GetDiagnosis(int cid)
        {
            var diagnosis = db.AI_Diagnoses
                .Where(d => d.Cid == cid)
                .ToList();

            return Ok(diagnosis);
        }
        [HttpGet("{cid}/fertilizers")]
        public IActionResult GetFertilizers(int cid)
        {
            var fertilizers = db.CROPs
                .Where(c => c.Cid == cid)
                .SelectMany(c => c.Frs)
                .ToList();

            return Ok(fertilizers);
        }
    }
}
