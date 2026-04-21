using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        public plantController(farContext db)
        {
            this.db = db;
        }

        //list
        [HttpGet]
        public ActionResult getall()
        {
            return Ok(db.PLANTs.ToList());
        }

        //get by id
        [HttpGet("{id}")]
        public ActionResult getbyid(int id)
        {
            PLANT? b = db.PLANTs.Find(id);

            if (b == null) return NotFound();
            else return Ok(b);
        }

        // GET api/plant/{pid}
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

        //grows in    for plant
        [HttpGet("growin")]
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

