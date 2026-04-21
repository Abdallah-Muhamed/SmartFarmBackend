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
    public class seasonController : ControllerBase
    {
         farContext db;

        public seasonController(farContext context)
        {
            db = context;
        }

        // get all
        [HttpGet]
        public IActionResult GetAll()
        {
            var seasons = db.SEASONs
                .Include(s => s.GROWS_INs)
                .ThenInclude(g => g.PidNavigation)
                .Select(s => new SeasonDTO
                {
                    Sid = s.Sid,
                    Name = s.Name,
                    Description = s.Description,

                    Plants = s.GROWS_INs
                        .Select(g => g.PidNavigation.Name)
                        .ToList()
                })
                .ToList();

            return Ok(seasons);
        }

        // GET by id
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var season = db.SEASONs
                .Include(s => s.GROWS_INs)
                .ThenInclude(g => g.PidNavigation)
                .Where(s => s.Sid == id)
                .Select(s => new SeasonDTO
                {
                    Sid = s.Sid,
                    Name = s.Name,
                    Description = s.Description,

                    Plants = s.GROWS_INs
                        .Select(g => g.PidNavigation.Name)
                        .ToList()
                })
                .FirstOrDefault();

            if (season == null)
                return NotFound();

            return Ok(season);
        }
    }
}

