using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Smart_Farm.DTOS;
using Smart_Farm.Models;
using Microsoft.EntityFrameworkCore;

namespace Smart_Farm.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DiseaseController : ControllerBase
    {
         farContext db;

        public DiseaseController(farContext db)
        {
            this.db=db;
        }

        // GET: api/diseases
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DiseaseDto>>> GetDiseases()
        {
            var diseases = await db.Diseases
                .Select(d => new DiseaseDto
                {
                    Did = d.Did,
                    Name = d.Name,
                    Cause = d.Cause,
                    Symptoms = d.Symptoms,
                    Treatment = d.Treatment
                })
                .ToListAsync();

            return Ok(diseases);
        }

        // GET: api/diseases/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DiseaseDto>> GetDisease(int id)
        {
            var disease = await db.Diseases
                .Where(d => d.Did == id)
                .Select(d => new DiseaseDto
                {
                    Did = d.Did,
                    Name = d.Name,
                    Cause = d.Cause,
                    Symptoms = d.Symptoms,
                    Treatment = d.Treatment
                })
                .FirstOrDefaultAsync();

            if (disease == null)
                return NotFound();

            return Ok(disease);
        }

        // POST: api/diseases
        [HttpPost]
        public async Task<ActionResult<DiseaseDto>> CreateDisease(DiseaseDto dto)
        {
            var disease = new Disease
            {
                Name = dto.Name,
                Cause = dto.Cause,
                Symptoms = dto.Symptoms,
                Treatment = dto.Treatment
            };

            db.Diseases.Add(disease);
            await db.SaveChangesAsync();

            dto.Did = disease.Did;

            return CreatedAtAction(nameof(GetDisease), new { id = disease.Did }, dto);
        }
    }
}
