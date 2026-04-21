using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Smart_Farm.DTOS;
using Smart_Farm.Infrastructure.Security;
using Smart_Farm.Models;

namespace Smart_Farm.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class taskController : ControllerBase
    {
        farContext db;

        public taskController(farContext db)
        {
            this.db = db;
        }

        //list
        [HttpGet]
        public ActionResult getall()
        {
            var items = db.Tasks.Select(t => new TaskResponseDto
            {
                Task_id = t.Task_id,
                Date = t.Date,
                Label = t.Label,
                Content = t.Content,
                State = t.State,
                Uid = t.Uid
            }).ToList();
            return Ok(items);
        }

        [HttpGet("me")]
        public ActionResult GetMine()
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            var items = db.Tasks
                .Where(t => t.Uid == uid)
                .Select(t => new TaskResponseDto
                {
                    Task_id = t.Task_id,
                    Date = t.Date,
                    Label = t.Label,
                    Content = t.Content,
                    State = t.State,
                    Uid = t.Uid
                })
                .ToList();

            return Ok(items);
        }

        //get by id
        [HttpGet("{id}")]
        public ActionResult getbyid(int id)
        {
            Models.Task? b = db.Tasks.Find(id);

            if (b == null) return NotFound();
            return Ok(new TaskResponseDto
            {
                Task_id = b.Task_id,
                Date = b.Date,
                Label = b.Label,
                Content = b.Content,
                State = b.State,
                Uid = b.Uid
            });
        }

        //Edit

        //DELETE
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            Models.Task? b = db.Tasks.Find(id);
            if (b == null) return NotFound();
            if (b.Uid != uid) return Forbid();
            db.Tasks.Remove(b);
            db.SaveChanges();
            return Ok(new { id = b.Task_id, deleted = true });


        }
        // add
        [HttpPost]
        public ActionResult post(TaskRequestDto b)
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            if (b == null) return BadRequest("Tasks is null");
            if (!ModelState.IsValid) return BadRequest();
            var entity = new Models.Task
            {
                Date = b.Date,
                Label = b.Label,
                Content = b.Content,
                State = b.State,
                Uid = uid
            };
            db.Tasks.Add(entity);
            db.SaveChanges();
            return CreatedAtAction(nameof(getbyid), new { id = entity.Task_id }, new TaskResponseDto
            {
                Task_id = entity.Task_id,
                Date = entity.Date,
                Label = entity.Label,
                Content = entity.Content,
                State = entity.State,
                Uid = entity.Uid
            });


        }
        [HttpPut("{id}")]
        public ActionResult edit(TaskRequestDto b, int id)
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            if (b == null) return BadRequest("tasks is null");
            var entity = db.Tasks.Find(id);
            if (entity == null) return NotFound();
            if (entity.Uid != uid) return Forbid();
            entity.Date = b.Date;
            entity.Label = b.Label;
            entity.Content = b.Content;
            entity.State = b.State;
            db.SaveChanges();
            return NoContent();

        }
    }
}

