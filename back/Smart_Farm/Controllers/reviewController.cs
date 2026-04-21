using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.DTOS;
using Smart_Farm.Infrastructure.Security;
using Smart_Farm.Models;

namespace Smart_Farm.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class reviewController(farContext db) : ControllerBase
{
    [HttpGet("product/{pid:int}")]
    public IActionResult GetByProduct(int pid)
    {
        var reviews = db.REVIEWs
            .AsNoTracking()
            .Where(r => r.Pid == pid)
            .OrderByDescending(r => r.CreatedUtc)
            .Select(r => new ReviewResponseDto
            {
                Rid = r.Rid,
                Pid = r.Pid,
                Uid = r.Uid,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedUtc = r.CreatedUtc,
                ReviewerName = r.UidNavigation != null ? (r.UidNavigation.First_name + " " + r.UidNavigation.Last_name) : null
            })
            .ToList();

        return Ok(reviews);
    }

    [HttpPost]
    public IActionResult Create(ReviewRequestDto request)
    {
        if (!UserClaims.TryGetUid(User, out var uid))
            return Unauthorized();

        if (request is null)
            return BadRequest();

        var entity = new REVIEW
        {
            Pid = request.Pid,
            Uid = uid,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedUtc = DateTime.UtcNow
        };

        db.REVIEWs.Add(entity);
        db.SaveChanges();

        return Ok(new { entity.Rid });
    }
}

