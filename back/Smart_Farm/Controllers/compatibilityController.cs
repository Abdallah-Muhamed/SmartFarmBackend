using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Smart_Farm.Models;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.DTOS;

namespace Smart_Farm.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class compatibilityController : ControllerBase
    {
         farContext db;

        public compatibilityController(farContext context)
        {
            db = context;
        }

        // GET api/compatibility
        [HttpGet]
        public IActionResult GetAllCompatibility()
        {
            var data = db.COMPATIBILITies
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
        // GET api/compatibility/plant/{pid}
        [HttpGet("plant/{pid}")]
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
    }
}
