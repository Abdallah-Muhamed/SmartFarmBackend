using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.Models;

namespace Smart_Farm.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class fertilizerController : ControllerBase
    {
        farContext db;

        public fertilizerController(farContext db)
        {
            this.db = db;
        }

        //list
        [HttpGet]
        public ActionResult getall()
        {
            return Ok(db.FERTILIZERs.ToList());
        }

        //get by id
        [HttpGet("{id}")]
        public ActionResult getbyid(int id)
        {
            FERTILIZER? b = db.FERTILIZERs.Find(id);

            if (b == null) return NotFound();
            else return Ok(b);
        }



    }
}