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
public class ProductController(farContext db) : ControllerBase
{
    private readonly farContext _db = db;

    // GET: api/product
    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] double? minPrice,
        [FromQuery] double? maxPrice,
        [FromQuery] double? minRating,
        [FromQuery] string? city)
    {
        var query = _db.PRODUCTs.AsQueryable();

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

        var products = await query
            .AsNoTracking()
            .Select(p => new ProductResponseDto
            {
                Pid = p.Pid,
                Description = p.Description,
                Price = p.Price,
                Added_date = p.Added_date,
                Quantity = p.Quantity,
                Uid = p.Uid,
                Category = p.Category,
                Rating = p.Rating
            })
            .ToListAsync();

        return Ok(products);
    }

    // GET: api/product/me
    [HttpGet("me")]
    public async Task<ActionResult> GetMine()
    {
        var uid = UserClaims.RequireUid(User);

        var items = await _db.PRODUCTs
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
                Category = p.Category,
                Rating = p.Rating
            })
            .ToListAsync();

        return Ok(items);
    }

    // GET: api/product/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        var product = await _db.PRODUCTs
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Pid == id);

        if (product == null)
            return NotFound();

        return Ok(new ProductResponseDto
        {
            Pid = product.Pid,
            Description = product.Description,
            Price = product.Price,
            Added_date = product.Added_date,
            Quantity = product.Quantity,
            Uid = product.Uid,
            Category = product.Category,
            Rating = product.Rating
        });
    }

    // POST: api/product
    [HttpPost]
    public async Task<ActionResult> Create(ProductRequestDto dto)
    {
        var uid = UserClaims.RequireUid(User);

        var entity = new PRODUCT
        {
            Description = dto.Description,
            Price = dto.Price,
            Added_date = dto.Added_date,
            Quantity = dto.Quantity,
            Uid = uid,
            Category = dto.Category,
            Rating = dto.Rating,
            CreatedAt = DateTime.UtcNow
        };

        _db.PRODUCTs.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Pid }, new ProductResponseDto
        {
            Pid = entity.Pid,
            Description = entity.Description,
            Price = entity.Price,
            Added_date = entity.Added_date,
            Quantity = entity.Quantity,
            Uid = entity.Uid,
            Category = entity.Category,
            Rating = entity.Rating
        });
    }

    // PUT: api/product/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, ProductRequestDto dto)
    {
        var uid = UserClaims.RequireUid(User);

        var entity = await _db.PRODUCTs.FirstOrDefaultAsync(p => p.Pid == id);

        if (entity == null)
            return NotFound();

        if (entity.Uid != uid)
            return Forbid();

        entity.Description = dto.Description;
        entity.Price = dto.Price;
        entity.Added_date = dto.Added_date;
        entity.Quantity = dto.Quantity;
        entity.Category = dto.Category;
        entity.Rating = dto.Rating;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("all")]
    public async Task<ActionResult> DeleteAll()
    {
        var uid = UserClaims.RequireUid(User);
        var deletedCount = await _db.PRODUCTs.Where(p => p.Uid == uid).ExecuteDeleteAsync();
        return Ok(new { deletedCount });
    }

    // DELETE: api/product/{id}
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var uid = UserClaims.RequireUid(User);

        var entity = await _db.PRODUCTs
            .FirstOrDefaultAsync(p => p.Pid == id);

        if (entity == null)
            return NotFound();

        if (entity.Uid != uid)
            return Forbid();

        _db.PRODUCTs.Remove(entity);
        await _db.SaveChangesAsync();

        return Ok(new { id, deleted = true });
    }
}