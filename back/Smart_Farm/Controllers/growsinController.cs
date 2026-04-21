using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smart_Farm.DTOS;
using Smart_Farm.Models;
using Microsoft.EntityFrameworkCore;
namespace Smart_Farm.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class growsinController : ControllerBase
    {
         farContext db;

        public growsinController(farContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Get()
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
    }
}
