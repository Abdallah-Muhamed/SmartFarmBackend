using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Smart_Farm.DTOS;
using Smart_Farm.Models;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.Infrastructure.Security;

namespace Smart_Farm.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class irrigationController : ControllerBase
    {
         farContext db;

        public irrigationController(farContext context)
        {
            db = context;
        }

        // display all
        [HttpGet]
        public IActionResult GetAll()
        {
            var irrigations = db.IRRIGATIONs
                .Include(i => i.CidNavigation)
                .Include(i => i.SisNavigation)
                .Select(i => new IrrigationDTO
                {
                    Iid = i.Iid,
                    Irrigation_name = i.Irrigation_name,
                    Description = i.Description,
                    Frequency_unit = i.Frequency_unit,
                    Frequency_value = i.Frequency_value,
                    Water_amount = i.Water_amount,
                    Cid = i.Cid,
                    Sis = i.Sis,
                    CropName = i.CidNavigation.Notes,
                    StageName = i.SisNavigation.Name_stage
                })
                .ToList();

            return Ok(irrigations);
        }

        // display by id
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var irrigation = db.IRRIGATIONs
                .Include(i => i.CidNavigation)
                .Include(i => i.SisNavigation)
                .Where(i => i.Iid == id)
                .Select(i => new IrrigationDTO
                {
                    Iid = i.Iid,
                    Irrigation_name = i.Irrigation_name,
                    Description = i.Description,
                    Frequency_unit = i.Frequency_unit,
                    Frequency_value = i.Frequency_value,
                    Water_amount = i.Water_amount,
                    Cid = i.Cid,
                    Sis = i.Sis,
                    CropName = i.CidNavigation.Notes,
                    StageName = i.SisNavigation.Name_stage
                })
                .FirstOrDefault();

            if (irrigation == null)
                return NotFound();

            return Ok(irrigation);
        }

        // display all irrigation for specific crop=id
        [HttpGet("crop/{cid}")]
        public IActionResult GetByCrop(int cid)
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            var auth = CropAuthorization.EnsureCropOwnedByUser(db, cid, uid);
            if (auth is not null) return auth;

            var irrigations = db.IRRIGATIONs
                .Where(i => i.Cid == cid)
                .Select(i => new IrrigationDTO
                {CropName=i.CidNavigation.Notes,
                    Iid = i.Iid,
                    Irrigation_name = i.Irrigation_name,
                    Description = i.Description,
                    Frequency_unit = i.Frequency_unit,
                    Frequency_value = i.Frequency_value,
                    Water_amount = i.Water_amount,
                    Sis = i.Sis,
                    Cid = i.Cid,
                    StageName=i.SisNavigation.Name_stage
                    
                })
                .ToList();

            return Ok(irrigations);
        }
        //display all irrigation for specific stage

        [HttpGet("stage/{sid}")]
        public IActionResult GetByStage(int sid)
        {
            var irrigations = db.IRRIGATIONs
                .Where(i => i.Sis == sid)
                .Select(i => new IrrigationDTO
                {
                    Iid = i.Iid,
                    Irrigation_name = i.Irrigation_name,
                    Description = i.Description,
                    Frequency_unit = i.Frequency_unit,
                    Frequency_value = i.Frequency_value,
                    Water_amount = i.Water_amount,
                    Sis = i.Sis,
                    Cid = i.Cid,
                    StageName=i.SisNavigation.Name_stage,
                    CropName=i.CidNavigation.Notes
                })
                .ToList();

            return Ok(irrigations);
        }
        //DELETE
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            IRRIGATION? b = db.IRRIGATIONs.Find(id);
            if (b == null) return NotFound();

            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            if (b.Cid is null)
                return BadRequest("Irrigation is missing crop id.");

            var auth = CropAuthorization.EnsureCropOwnedByUser(db, b.Cid.Value, uid);
            if (auth is not null) return auth;

            db.IRRIGATIONs.Remove(b);
            db.SaveChanges();
            return Ok(new { id = b.Iid, deleted = true });


        }
        // add
        [HttpPost]
        public ActionResult post(IrrigationRequestDto b)
        {
            if (b == null) return BadRequest("irrigations is null");
            if (!ModelState.IsValid) return BadRequest();

            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            if (b.Cid is null)
                return BadRequest("cid is required.");

            var auth = CropAuthorization.EnsureCropOwnedByUser(db, b.Cid.Value, uid);
            if (auth is not null) return auth;

            var entity = new IRRIGATION
            {
                Irrigation_name = b.Irrigation_name,
                Description = b.Description,
                Frequency_unit = b.Frequency_unit,
                Frequency_value = b.Frequency_value,
                Water_amount = b.Water_amount,
                Sis = b.Sis,
                Cid = b.Cid
            };
            db.IRRIGATIONs.Add(entity);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = entity.Iid }, new { entity.Iid });


        }
        //edit
        [HttpPut("{id}")]
        public ActionResult edit(IrrigationRequestDto b, int id)
        {
            if (b == null) return BadRequest("irrigations is null");
            var entity = db.IRRIGATIONs.Find(id);
            if (entity == null) return NotFound();

            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            var cid = b.Cid ?? entity.Cid;
            if (cid is null)
                return BadRequest("cid is required.");

            var auth = CropAuthorization.EnsureCropOwnedByUser(db, cid.Value, uid);
            if (auth is not null) return auth;

            entity.Irrigation_name = b.Irrigation_name;
            entity.Description = b.Description;
            entity.Frequency_unit = b.Frequency_unit;
            entity.Frequency_value = b.Frequency_value;
            entity.Water_amount = b.Water_amount;
            entity.Sis = b.Sis;
            entity.Cid = cid;
            db.SaveChanges();
            return NoContent();

        }
    }
}


    

