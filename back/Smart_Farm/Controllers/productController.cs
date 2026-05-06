using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smart_Farm.DTOS;
using Smart_Farm.Infrastructure.Security;
using Smart_Farm.Models;
using Microsoft.EntityFrameworkCore;

namespace Smart_Farm.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class productController : ControllerBase
    {
        farContext db;

        public productController(farContext db)
        {
            this.db = db;
        }

        // ─── GET api/product ─────────────────────────────────────────────────
        [HttpGet]
        public ActionResult GetAll(
            [FromQuery] string? category,
            [FromQuery] double? minPrice,
            [FromQuery] double? maxPrice,
            [FromQuery] double? minRating,
            [FromQuery] string? city)
        {
            var query = db.PRODUCTs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category == category);
            if (minPrice.HasValue)
                query = query.Where(p => p.Price != null && (double)p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price != null && (double)p.Price <= maxPrice.Value);
            if (minRating.HasValue)
                query = query.Where(p => p.Rating != null && p.Rating >= minRating.Value);
            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(p => p.UidNavigation != null && p.UidNavigation.City_name == city);

            var products = query
                .AsNoTracking()
                .Select(p => new ProductResponseDto
                {
                    Pid = p.Pid,
                    Description = p.Description,
                    Price = p.Price,
                    Added_date = p.Added_date,
                    Quantity = p.Quantity,
                    Uid = p.Uid,
                    Cid = p.Cid,
                    Category = p.Category,
                    Rating = p.Rating,
                })
                .ToList();

            return Ok(products);
        }

        // ─── GET api/product/me ──────────────────────────────────────────────
        [HttpGet("me")]
        public ActionResult GetMine()
        {
            var uid = UserClaims.RequireUid(User);

            var items = db.PRODUCTs
                .AsNoTracking()
                .Where(p => p.Uid == uid)
                .Select(p => new ProductResponseDto
                {
                    Pid = p.Pid,
                    Description = p.Description,
                    Price = p.Price,
                    Added_date = p.Added_date,
                    Quantity = p.Quantity,
                    Uid = p.Uid,
                    Cid = p.Cid,
                    Category = p.Category,
                    Rating = p.Rating,
                })
                .ToList();

            return Ok(items);
        }

        // ─── GET api/product/{id} ────────────────────────────────────────────
        [HttpGet("{id}")]
        public ActionResult GetById(int id)
        {
            PRODUCT? b = db.PRODUCTs.Find(id);
            if (b == null) return NotFound();

            return Ok(new ProductResponseDto
            {
                Pid = b.Pid,
                Description = b.Description,
                Price = b.Price,
                Added_date = b.Added_date,
                Quantity = b.Quantity,
                Uid = b.Uid,
                Cid = b.Cid,
                Category = b.Category,
                Rating = b.Rating,
            });
        }

        // ─── POST api/product ────────────────────────────────────────────────
        [HttpPost]
        public ActionResult Post(ProductRequestDto b)
        {
            var uid = UserClaims.RequireUid(User);
            if (b == null) return BadRequest("product is null");
            if (!ModelState.IsValid) return BadRequest();

            var entity = new PRODUCT
            {
                Description = b.Description,
                Price = b.Price,
                Added_date = b.Added_date,
                Quantity = b.Quantity,
                Uid = uid,
                Cid = b.Cid,
                Category = b.Category,
                Rating = b.Rating,
                CreatedAt = DateTime.UtcNow
            };
            db.PRODUCTs.Add(entity);
            db.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = entity.Pid }, new ProductResponseDto
            {
                Pid = entity.Pid,
                Description = entity.Description,
                Price = entity.Price,
                Added_date = entity.Added_date,
                Quantity = entity.Quantity,
                Uid = entity.Uid,
                Cid = entity.Cid,
                Category = entity.Category,
                Rating = entity.Rating
            });
        }

        // ─── PUT api/product/{id} ────────────────────────────────────────────
        [HttpPut("{id}")]
        public ActionResult Edit(ProductRequestDto b, int id)
        {
            var uid = UserClaims.RequireUid(User);
            if (b == null) return BadRequest("product is null");

            var entity = db.PRODUCTs.Find(id);
            if (entity == null) return NotFound();
            if (entity.Uid != uid) return Forbid();

            entity.Description = b.Description;
            entity.Price = b.Price;
            entity.Added_date = b.Added_date;
            entity.Quantity = b.Quantity;
            entity.Cid = b.Cid;
            entity.Category = b.Category;
            entity.Rating = b.Rating;

            db.SaveChanges();
            return NoContent();
        }

        // ─── DELETE api/product/{id} ─────────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var uid = UserClaims.RequireUid(User);

            var entity = await db.PRODUCTs
                .FirstOrDefaultAsync(p => p.Pid == id, cancellationToken);

            if (entity == null) return NotFound();
            if (entity.Uid != uid) return Forbid();

            db.PRODUCTs.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);

            return Ok(new { id = entity.Pid, deleted = true });
        }
    }
}