using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.DTOS;
using Smart_Farm.Models;

namespace Smart_Farm.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class plantController : ControllerBase
    {
        farContext db;
        private readonly Cloudinary _cloudinary;

        public plantController(farContext db, IConfiguration configuration)
        {
            this.db = db;
            _cloudinary = new Cloudinary(new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            ));
        }

        // ─── GET api/plant ────────────────────────────────────────────────────
        [HttpGet]
        public ActionResult GetAll()
        {
            var plants = db.PLANTs
                .AsNoTracking()
                .Select(p => new PlantResponseDto
                {
                    Pid = p.Pid,
                    PhotoUrl = p.PhotoUrl,
                    Name = p.Name,
                    Description = p.Description,
                    Seed_type = p.Seed_type,
                    Fertilizer_need = p.Fertilizer_need,
                    Days_to_harvest = p.Days_to_harvest,
                    Season = p.Season,
                    Humidity_range = p.Humidity_range,
                    Water_need = p.Water_need,
                    Soil_type = p.Soil_type,
                    Temperature_range = p.Temperature_range
                })
                .ToList();

            return Ok(plants);
        }

        // ─── GET api/plant/{id} ───────────────────────────────────────────────
        [HttpGet("{id}")]
        public ActionResult GetById(int id)
        {
            PLANT? b = db.PLANTs.Find(id);
            if (b == null) return NotFound();

            return Ok(new PlantResponseDto
            {
                Pid = b.Pid,
                PhotoUrl = b.PhotoUrl,
                Name = b.Name,
                Description = b.Description,
                Seed_type = b.Seed_type,
                Fertilizer_need = b.Fertilizer_need,
                Days_to_harvest = b.Days_to_harvest,
                Season = b.Season,
                Humidity_range = b.Humidity_range,
                Water_need = b.Water_need,
                Soil_type = b.Soil_type,
                Temperature_range = b.Temperature_range
            });
        }

        

        // ─── GET api/plant/compatibility/{pid} ───────────────────────────────
        [HttpGet("compatibility/{pid}")]
        public IActionResult GetCompatibilityByPlant(int pid)
        {
            var data = db.COMPATIBILITies
                .Where(c => c.Pid == pid)
                .Include(c => c.Fr)
                .Include(c => c.PidNavigation)
                .Select(c => new DTOS.COMPATIBILITY
                {
                    FertilizerName = c.Fr.Fertilizer_name,
                    PlantName = c.PidNavigation.Name,
                    Rate = c.Rate
                })
                .ToList();

            return Ok(data);
        }

        // ─── GET api/plant/growin ─────────────────────────────────────────────
        [HttpGet("growin")]
        public IActionResult GetGrowsIn()
        {
            var data = db.GROWS_INs
                .Include(x => x.PidNavigation)
                .Include(x => x.SidNavigation)
                .Select(x => new Grows_in
                {
                    PlantName = x.PidNavigation.Name,
                    SeasonName = x.SidNavigation.Name,
                    Description = x.Plant_in_season_description,
                    Rate = x.Rate
                })
                .ToList();

            return Ok(data);
        }

        // ─── POST api/plant/seed-stages (Allows anonymous seeding) ────────────
        [AllowAnonymous]
        [HttpPost("seed-stages")]
        public async Task<IActionResult> SeedMissingStages()
        {
            var plantsWithNoStages = await db.PLANTs
                .Where(p => !db.PLANT_STAGEs.Any(s => s.Pid == p.Pid))
                .ToListAsync();

            if (plantsWithNoStages.Count == 0)
            {
                return Ok(new { message = "All plants already have growth stages configured." });
            }

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                int stagesInserted = 0;
                int templatesInserted = 0;

                foreach (var plant in plantsWithNoStages)
                {
                    // Define standard stages
                    var stages = new List<PLANT_STAGE>
                    {
                        new PLANT_STAGE { Pid = plant.Pid, Name_stage = "الإنبات", Stage_order = 1, Duration_days = 10, Description = "مرحلة الإنبات والنمو الأولي للشتلة" },
                        new PLANT_STAGE { Pid = plant.Pid, Name_stage = "النمو الخضري", Stage_order = 2, Duration_days = 20, Description = "مرحلة النمو الخضري للأوراق والسيقان" },
                        new PLANT_STAGE { Pid = plant.Pid, Name_stage = "التزهير", Stage_order = 3, Duration_days = 15, Description = "مرحلة الإزهار وبداية التلقيح" },
                        new PLANT_STAGE { Pid = plant.Pid, Name_stage = "العقد", Stage_order = 4, Duration_days = 25, Description = "مرحلة عقد وتكون وتضخم الثمار" },
                        new PLANT_STAGE { Pid = plant.Pid, Name_stage = "النضج", Stage_order = 5, Duration_days = 15, Description = "مرحلة نضج الثمار واقتراب الحصاد" }
                    };

                    db.PLANT_STAGEs.AddRange(stages);
                    await db.SaveChangesAsync();
                    stagesInserted += stages.Count;

                    // Add templates for each stage
                    var templates = new List<PLANT_IRRIGATION_TEMPLATE>
                    {
                        new PLANT_IRRIGATION_TEMPLATE
                        {
                            Pid = plant.Pid, PSid = stages[0].PSid, Irrigation_name = $"ري إنبات {plant.Name}",
                            Water_amount = 30, Frequency_value = 5, Frequency_unit = "days",
                            Description = "ري خفيف لإنبات البذور", Kc = 0.40m, p_fraction = 0.50m, Zr_m = 0.15m
                        },
                        new PLANT_IRRIGATION_TEMPLATE
                        {
                            Pid = plant.Pid, PSid = stages[1].PSid, Irrigation_name = $"ري نمو {plant.Name}",
                            Water_amount = 40, Frequency_value = 4, Frequency_unit = "days",
                            Description = "ري معتدل للنمو الخضري", Kc = 0.70m, p_fraction = 0.50m, Zr_m = 0.40m
                        },
                        new PLANT_IRRIGATION_TEMPLATE
                        {
                            Pid = plant.Pid, PSid = stages[2].PSid, Irrigation_name = $"ري تزهير {plant.Name}",
                            Water_amount = 45, Frequency_value = 3, Frequency_unit = "days",
                            Description = "ري للأزهار وتثبيت العقد", Kc = 1.05m, p_fraction = 0.50m, Zr_m = 0.60m
                        },
                        new PLANT_IRRIGATION_TEMPLATE
                        {
                            Pid = plant.Pid, PSid = stages[3].PSid, Irrigation_name = $"ري عقد {plant.Name}",
                            Water_amount = 50, Frequency_value = 3, Frequency_unit = "days",
                            Description = "ري لنمو وتكبير الثمار", Kc = 1.15m, p_fraction = 0.50m, Zr_m = 0.80m
                        },
                        new PLANT_IRRIGATION_TEMPLATE
                        {
                            Pid = plant.Pid, PSid = stages[4].PSid, Irrigation_name = $"ري نضج {plant.Name}",
                            Water_amount = 40, Frequency_value = 4, Frequency_unit = "days",
                            Description = "تقليل الري لتهيئة المحصول للحصاد", Kc = 0.90m, p_fraction = 0.50m, Zr_m = 0.90m
                        }
                    };

                    db.PLANT_IRRIGATION_TEMPLATEs.AddRange(templates);
                    await db.SaveChangesAsync();
                    templatesInserted += templates.Count;
                }

                await transaction.CommitAsync();
                return Ok(new
                {
                    message = $"Successfully seeded {plantsWithNoStages.Count} plants.",
                    stagesSeeded = stagesInserted,
                    templatesSeeded = templatesInserted
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ─── Helper ───────────────────────────────────────────────────────────
        private static string? ExtractPublicId(string url)
        {
            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/');
                var uploadIdx = Array.IndexOf(segments, "upload");
                if (uploadIdx < 0) return null;
                var start = uploadIdx + 1;
                if (start < segments.Length && segments[start].StartsWith('v') &&
                    long.TryParse(segments[start][1..], out _))
                    start++;
                var publicIdWithExt = string.Join("/", segments[start..]);
                var dot = publicIdWithExt.LastIndexOf('.');
                return dot >= 0 ? publicIdWithExt[..dot] : publicIdWithExt;
            }
            catch { return null; }
        }
    }
}