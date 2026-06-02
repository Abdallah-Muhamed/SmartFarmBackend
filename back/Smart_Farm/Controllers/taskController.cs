using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public ActionResult GetMine()
        {
            var uid = UserClaims.RequireUid(User);

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
            var uid = UserClaims.RequireUid(User);
            Models.Task? b = db.Tasks.Find(id);

            if (b == null) return NotFound();
            if (b.Uid != uid) return Forbid();
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

        [HttpDelete("all")]
        public async Task<ActionResult> DeleteAll(CancellationToken ct)
        {
            var uid = UserClaims.RequireUid(User);
            var deletedCount = await db.Tasks.Where(t => t.Uid == uid).ExecuteDeleteAsync(ct);
            return Ok(new { deletedCount });
        }

        //DELETE
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct)
        {
            var uid = UserClaims.RequireUid(User);

            Models.Task? b = await db.Tasks.FindAsync(new object[] { id }, ct);
            if (b == null) return NotFound();
            if (b.Uid != uid) return Forbid();
            db.Tasks.Remove(b);
            await db.SaveChangesAsync(ct);
            return Ok(new { id = b.Task_id, deleted = true });
        }
        // add
        [HttpPost]
        public async Task<ActionResult> post(TaskRequestDto b, CancellationToken ct)
        {
            var uid = UserClaims.RequireUid(User);

            if (b == null) return BadRequest("Tasks is null");
            if (!ModelState.IsValid) return BadRequest();
            var entity = new Models.Task
            {
                Date = b.Date,
                Label = b.Label,
                Content = b.Content,
                State = b.State ?? "pending",
                Uid = uid,
                CreatedAt = DateTime.UtcNow
            };
            db.Tasks.Add(entity);
            await db.SaveChangesAsync(ct);
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
        public async Task<ActionResult> edit(TaskRequestDto b, int id, CancellationToken ct)
        {
            var uid = UserClaims.RequireUid(User);

            if (b == null) return BadRequest("tasks is null");
            var entity = await db.Tasks.FindAsync(new object[] { id }, ct);
            if (entity == null) return NotFound();
            if (entity.Uid != uid) return Forbid();
            entity.Date = b.Date;
            entity.Label = b.Label;
            entity.Content = b.Content;
            entity.State = b.State;
            await db.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpPatch("{id}/state")]
        public async Task<ActionResult> PatchState(int id, [FromBody] string state, CancellationToken ct)
        {
            var uid = UserClaims.RequireUid(User);

            var validStates = new[] { "pending", "done", "skipped" };
            if (string.IsNullOrWhiteSpace(state) || !validStates.Contains(state))
                return BadRequest($"State must be one of: {string.Join(", ", validStates)}");

            var entity = await db.Tasks.FindAsync(new object[] { id }, ct);
            if (entity == null) return NotFound();
            if (entity.Uid != uid) return Forbid();

            entity.State = state;
            await db.SaveChangesAsync(ct);
            return Ok(new { Task_id = entity.Task_id, State = entity.State });
        }
    }
}

