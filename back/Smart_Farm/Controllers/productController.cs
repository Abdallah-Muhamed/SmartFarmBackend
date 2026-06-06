using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.DTOS;
using Smart_Farm.Infrastructure.Persistence;
using Smart_Farm.Infrastructure.Security;
using Smart_Farm.Models;

namespace Smart_Farm.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductController(farContext db, CloudinaryService cloudinary) : ControllerBase
{
    private readonly farContext _db = db;

    private static string BuildSellerName(USER? user)
    {
        if (user is null) return "بائع";
        return $"{user.First_name} {user.Last_name}".Trim();
    }

    private static ProductResponseDto MapProduct(PRODUCT p) => new()
    {
        Pid = p.Pid,
        Description = p.Description,
        Price = p.Price,
        Added_date = p.Added_date,
        Quantity = p.Quantity,
        Uid = p.Uid,
        Cid = p.Cid,
        FarmId = p.FarmId,
        PhotoUrl = ResolvePhotoUrl(p),
        Category = p.Category,
        Rating = p.Rating,
        SellerName = BuildSellerName(p.UidNavigation),
        SellerRole = p.UidNavigation?.Role,
        FarmName = p.FarmIdNavigation?.Name,
    };

    private static string? ResolvePhotoUrl(PRODUCT p)
    {
        if (!string.IsNullOrWhiteSpace(p.PhotoUrl))
            return p.PhotoUrl;

        return p.CidNavigation?.PidNavigation?.PhotoUrl;
    }

    private IQueryable<PRODUCT> ProductQuery() =>
        _db.PRODUCTs
            .AsNoTracking()
            .Include(p => p.UidNavigation)
            .Include(p => p.FarmIdNavigation)
            .Include(p => p.CidNavigation)
                .ThenInclude(c => c!.PidNavigation);

    // GET: api/product (public catalog)
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] double? minPrice,
        [FromQuery] double? maxPrice,
        [FromQuery] double? minRating,
        [FromQuery] string? city)
    {
        var query = ProductQuery();

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

        var products = await query.ToListAsync();
        return Ok(products.Select(MapProduct));
    }

    // GET: api/product/me
    [HttpGet("me")]
    public async Task<ActionResult> GetMine()
    {
        var uid = UserClaims.RequireUid(User);

        var items = await ProductQuery()
            .Where(p => p.Uid == uid)
            .ToListAsync();

        return Ok(items.Select(MapProduct));
    }

    // GET: api/product/{id} (public)
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        var product = await ProductQuery().FirstOrDefaultAsync(p => p.Pid == id);
        if (product is null)
            return NotFound();

        return Ok(MapProduct(product));
    }

    // POST: api/product/photo — upload product image before create
    [HttpPost("photo")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult> UploadPhoto(IFormFile image, CancellationToken ct)
    {
        if (image is null || image.Length == 0)
            return BadRequest("Image is required.");

        if (!image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only image files are allowed.");

        _ = UserClaims.RequireUid(User);

        await using var stream = image.OpenReadStream();
        var url = await cloudinary.UploadImageAsync(stream, image.FileName, "smart_farm/products", Guid.NewGuid().ToString("N"));
        return Ok(new { photoUrl = url });
    }

    // POST: api/product
    [HttpPost]
    public async Task<ActionResult> Create(ProductRequestDto dto)
    {
        var uid = UserClaims.RequireUid(User);

        if (dto.FarmId.HasValue)
        {
            var farm = await _db.FARMs.FirstOrDefaultAsync(f => f.FarmId == dto.FarmId.Value);
            if (farm is null)
                return BadRequest("Farm not found.");
            if (farm.Uid != uid)
                return Forbid();
        }

        if (dto.Cid.HasValue)
        {
            var crop = await _db.CROPs.FirstOrDefaultAsync(c => c.Cid == dto.Cid.Value);
            if (crop is null)
                return BadRequest("Crop not found.");
            if (crop.Uid != uid)
                return Forbid();
        }

        var entity = new PRODUCT
        {
            Description = dto.Description,
            Price = dto.Price,
            Added_date = dto.Added_date,
            Quantity = dto.Quantity,
            Uid = uid,
            Category = dto.Category,
            Rating = dto.Rating,
            FarmId = dto.FarmId,
            Cid = dto.Cid,
            PhotoUrl = dto.PhotoUrl,
            CreatedAt = DateTime.UtcNow
        };

        _db.PRODUCTs.Add(entity);
        await _db.SaveChangesAsync();

        var created = await ProductQuery().FirstAsync(p => p.Pid == entity.Pid);
        return CreatedAtAction(nameof(GetById), new { id = entity.Pid }, MapProduct(created));
    }

    // PUT: api/product/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, ProductRequestDto dto)
    {
        var uid = UserClaims.RequireUid(User);

        var entity = await _db.PRODUCTs.FirstOrDefaultAsync(p => p.Pid == id);
        if (entity is null)
            return NotFound();

        if (entity.Uid != uid)
            return Forbid();

        entity.Description = dto.Description;
        entity.Price = dto.Price;
        entity.Added_date = dto.Added_date;
        entity.Quantity = dto.Quantity;
        entity.Category = dto.Category;
        entity.Rating = dto.Rating;
        entity.FarmId = dto.FarmId;
        entity.Cid = dto.Cid;
        if (!string.IsNullOrWhiteSpace(dto.PhotoUrl))
            entity.PhotoUrl = dto.PhotoUrl;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("all")]
    public async Task<ActionResult> DeleteAll(CancellationToken ct)
    {
        var uid = UserClaims.RequireUid(User);

        var productIds = await _db.PRODUCTs
            .Where(p => p.Uid == uid)
            .Select(p => p.Pid)
            .ToListAsync(ct);

        await ProductDeletionHelper.RemoveDependenciesAsync(_db, productIds, ct);

        var deletedCount = await _db.PRODUCTs.Where(p => p.Uid == uid).ExecuteDeleteAsync(ct);
        return Ok(new { deletedCount });
    }

    // DELETE: api/product/{id}
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        var uid = UserClaims.RequireUid(User);

        var entity = await _db.PRODUCTs.FirstOrDefaultAsync(p => p.Pid == id, ct);
        if (entity is null)
            return NotFound();

        if (entity.Uid != uid)
            return Forbid();

        if (!string.IsNullOrWhiteSpace(entity.PhotoUrl))
            await cloudinary.TryDeleteByUrlAsync(entity.PhotoUrl);

        await ProductDeletionHelper.RemoveDependenciesAsync(_db, [entity.Pid], ct);

        _db.PRODUCTs.Remove(entity);
        await _db.SaveChangesAsync(ct);

        return Ok(new { id, deleted = true });
    }
}
