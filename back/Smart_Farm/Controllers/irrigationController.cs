using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smart_Farm.DTOS;
using Smart_Farm.Models;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.Infrastructure.Security;
using Smart_Farm.Application.Abstractions;
using System.Linq;

namespace Smart_Farm.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IrrigationController(farContext context, IWaterBalanceService service) : ControllerBase
    {
        private readonly farContext db = context;
        private readonly IWaterBalanceService _service = service;

        [HttpGet]
        public IActionResult GetAll()
        {
            var uid = UserClaims.RequireUid(User);
            var irrigations = db.IRRIGATIONs
                .Include(i => i.CidNavigation)
                .Include(i => i.SisNavigation)
                .Where(i => i.CidNavigation != null && i.CidNavigation.Uid == uid)
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

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var uid = UserClaims.RequireUid(User);
            var irrigation = db.IRRIGATIONs
                .Include(i => i.CidNavigation)
                .Include(i => i.SisNavigation)
                .Where(i => i.Iid == id && i.CidNavigation != null && i.CidNavigation.Uid == uid)
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

            if (irrigation == null) return NotFound();
            return Ok(irrigation);
        }

        [HttpGet("crop/{cid}")]
        public async Task<IActionResult> GetAllStagesByCrop(int cid, CancellationToken cancellationToken)
        {
            var uid = UserClaims.RequireUid(User);

            var auth = CropAuthorization.EnsureCropOwnedByUser(db, cid, uid);
            if (auth is not null) return auth;

            var crop = await db.CROPs.FirstOrDefaultAsync(c => c.Cid == cid, cancellationToken);
            if (crop is null) return NotFound();

            var plantStages = await db.PLANT_STAGEs
                .Where(ps => ps.Pid == crop.Pid)
                .OrderBy(ps => ps.Stage_order)
                .ToListAsync(cancellationToken);

            if (plantStages.Count == 0)
                return NotFound(new { error = "No stages found for this plant." });

            var psids = plantStages.Select(ps => ps.PSid).ToList();

            var templates = await db.PLANT_IRRIGATION_TEMPLATEs
                .Where(t => t.PSid.HasValue && psids.Contains(t.PSid.Value)).ToListAsync(cancellationToken);

            var customIrrigations = await db.IRRIGATIONs
                .Where(i => i.Cid == cid)
                .Include(i => i.SisNavigation)
                .ToListAsync(cancellationToken);

            var cropStartDate = crop.Start_date ?? crop.LastBalanceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            int daysSincePlanting = today.DayNumber - cropStartDate.DayNumber;

            var results = new List<StageIrrigationRecommendationDto>();
            int cumulativeDays = 0;

            foreach (var stage in plantStages)
            {
                int stageStart = cumulativeDays;
                int stageDuration = stage.Duration_days;
                int stageEnd = cumulativeDays + stageDuration;

                bool isCurrentStage = daysSincePlanting >= stageStart && daysSincePlanting < stageEnd;

                var template = templates.FirstOrDefault(t => t.PSid == stage.PSid);

                int intervalDays = template?.Frequency_value ?? 3;
                string irrigationMethod = template?.Irrigation_name ?? "تنقيط";
                decimal waterAmount = template?.Water_amount ?? 50m;
                var customIrrig = customIrrigations
                    .FirstOrDefault(i => i.SisNavigation?.Name_stage == stage.Name_stage);

                if (customIrrig != null)
                {
                    intervalDays = customIrrig.Frequency_value ?? intervalDays;
                    irrigationMethod = customIrrig.Irrigation_name ?? irrigationMethod;
                    waterAmount = customIrrig.Water_amount ?? waterAmount;
                }

                if (irrigationMethod.Contains("تنقيط")) irrigationMethod = "تنقيط";
                else if (irrigationMethod.Contains("رش")) irrigationMethod = "رش";
                else if (irrigationMethod.Contains("ري")) irrigationMethod = "تنقيط";

                if (waterAmount == 0) waterAmount = 50m;

                decimal recommendedLiters = Math.Round(waterAmount * (crop.Area_size ?? 1m) * 1000m, 0);
                int durationMinutes = (int)Math.Max(15, Math.Round((double)recommendedLiters / 5000.0 / 5.0) * 5);

                results.Add(new StageIrrigationRecommendationDto
                {
                    StageOrder = stage.Stage_order,
                    StageName = stage.Name_stage,
                    Description = stage.Description,
                    DurationDays = stageDuration,
                    StageStartDay = stageStart,
                    StageEndDay = stageEnd,
                    IsCurrentStage = isCurrentStage,
                    IntervalDays = intervalDays,
                    IrrigationMethod = irrigationMethod,
                    RecommendedLiters = recommendedLiters,
                    DurationMinutes = durationMinutes
                });

                cumulativeDays = stageEnd;
            }

            return Ok(new AllStagesIrrigationResponseDto
            {
                CropId = cid,
                PlantingDate = cropStartDate.ToString("yyyy-MM-dd"),
                TotalDays = cumulativeDays,
                Stages = results
            });
        }

        [HttpGet("stage/{sid}")]
        public IActionResult GetByStage(int sid)
        {
            var uid = UserClaims.RequireUid(User);
            var irrigations = db.IRRIGATIONs
                .Where(i => i.Sis == sid && i.CidNavigation != null && i.CidNavigation.Uid == uid)
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
                    StageName = i.SisNavigation.Name_stage,
                    CropName = i.CidNavigation.Notes
                })
                .ToList();

            return Ok(irrigations);
        }

        [HttpDelete("all")]
        public async Task<ActionResult> DeleteAll(CancellationToken ct)
        {
            var uid = UserClaims.RequireUid(User);
            var deletedCount = await db.IRRIGATIONs
                .Where(i => i.Cid != null && db.CROPs.Any(c => c.Cid == i.Cid && c.Uid == uid))
                .ExecuteDeleteAsync(ct);
            return Ok(new { deletedCount });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var uid = UserClaims.RequireUid(User);

            var entity = db.IRRIGATIONs.Find(id);
            if (entity == null) return NotFound();

            if (entity.Cid is null)
                return BadRequest("Irrigation is missing crop id.");

            var auth = CropAuthorization.EnsureCropOwnedByUser(db, entity.Cid.Value, uid);
            if (auth is not null) return auth;

            db.IRRIGATIONs.Remove(entity);
            db.SaveChanges();
            return Ok(new { id = entity.Iid, deleted = true });
        }

        [HttpPost]
        public ActionResult Post(IrrigationRequestDto b)
        {
            if (b == null) return BadRequest("irrigations is null");
            if (!ModelState.IsValid) return BadRequest();

            var uid = UserClaims.RequireUid(User);

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

        [HttpPut("{id}")]
        public ActionResult Edit(IrrigationRequestDto b, int id)
        {
            if (b == null) return BadRequest("irrigations is null");

            var uid = UserClaims.RequireUid(User);
            var entity = db.IRRIGATIONs.Find(id);
            if (entity == null) return NotFound();

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