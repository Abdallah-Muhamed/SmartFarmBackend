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
    public class IrrigationstageController : ControllerBase
    {
         farContext db;

        public IrrigationstageController(farContext context)
        {
            db = context;
        }

        // display all
        [HttpGet]
        public IActionResult GetAll()
        {
            var uid = UserClaims.RequireUid(User);
            var stages = db.IRRIGATION_STAGEs
                .Include(s => s.CidNavigation)
                .Where(s => s.CidNavigation != null && s.CidNavigation.Uid == uid)
                .Select(s => new IrrigationStageDTO
                {
                    Sid = s.Sid,
                    Name_stage = s.Name_stage,
                    Stage_order = s.Stage_order,
                    Description = s.Description,
                    Cid = s.Cid,
                    CropName = s.CidNavigation.Notes
                })
                .ToList();

            return Ok(stages);
        }

        // display by id
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var uid = UserClaims.RequireUid(User);
            var stage = db.IRRIGATION_STAGEs
                .Include(s => s.CidNavigation)
                .Where(s => s.Sid == id && s.CidNavigation != null && s.CidNavigation.Uid == uid)
                .Select(s => new IrrigationStageDTO
                {
                    Sid = s.Sid,
                    Name_stage = s.Name_stage,
                    Stage_order = s.Stage_order,
                    Description = s.Description,
                    Cid = s.Cid,
                    CropName = s.CidNavigation.Notes
                })
                .FirstOrDefault();

            if (stage == null)
                return NotFound();

            return Ok(stage);
        }

        // Display all irrigation stages for specific crop=id
        [HttpGet("crop/{cid}")]
        public IActionResult GetStagesByCrop(int cid)
        {
            var uid = UserClaims.RequireUid(User);

            var auth = CropAuthorization.EnsureCropOwnedByUser(db, cid, uid);
            if (auth is not null) return auth;

            var stages = db.IRRIGATION_STAGEs
                .Where(s => s.Cid == cid)
                .Select(s => new IrrigationStageDTO
                {    CropName=s.CidNavigation.Notes,
                    Sid = s.Sid,
                    Name_stage = s.Name_stage,
                    Stage_order = s.Stage_order,
                    Description = s.Description,
                    Cid = s.Cid
                })
                .ToList();

            return Ok(stages);
        }
        // delete
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            IRRIGATION_STAGE? b = db.IRRIGATION_STAGEs.Find(id);
            if (b == null) return NotFound();

            var uid = UserClaims.RequireUid(User);

            if (b.Cid is null)
                return BadRequest("Stage is missing crop id.");

            var auth = CropAuthorization.EnsureCropOwnedByUser(db, b.Cid.Value, uid);
            if (auth is not null) return auth;

            db.IRRIGATION_STAGEs.Remove(b);
            db.SaveChanges();
            return Ok(new { id = b.Sid, deleted = true });


        }
        // add
        [HttpPost]
        public ActionResult post(IrrigationStageRequestDto b)
        {
            if (b == null) return BadRequest("stages is null");
            if (!ModelState.IsValid) return BadRequest();

            var uid = UserClaims.RequireUid(User);

            if (b.Cid is null)
                return BadRequest("cid is required.");

            var auth = CropAuthorization.EnsureCropOwnedByUser(db, b.Cid.Value, uid);
            if (auth is not null) return auth;

            var entity = new IRRIGATION_STAGE
            {
                Name_stage = b.Name_stage,
                Stage_order = b.Stage_order,
                Description = b.Description,
                Cid = b.Cid
            };
            db.IRRIGATION_STAGEs.Add(entity);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = entity.Sid }, new { entity.Sid });


        }
        //edit
        [HttpPut("{id}")]
        public ActionResult edit(IrrigationStageRequestDto b, int id)
        {
            if (b == null) return BadRequest("stages is null");
            var entity = db.IRRIGATION_STAGEs.Find(id);
            if (entity == null) return NotFound();

            var uid = UserClaims.RequireUid(User);

            var cid = b.Cid ?? entity.Cid;
            if (cid is null)
                return BadRequest("cid is required.");

            var auth = CropAuthorization.EnsureCropOwnedByUser(db, cid.Value, uid);
            if (auth is not null) return auth;

            entity.Name_stage = b.Name_stage;
            entity.Stage_order = b.Stage_order;
            entity.Description = b.Description;
            entity.Cid = cid;
            db.SaveChanges();
            return NoContent();

        }
    }
}
