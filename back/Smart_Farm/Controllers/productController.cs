using Microsoft.AspNetCore.Http;
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

        //list
        [HttpGet]
        public ActionResult getall(
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

            // NOTE: city filter requires linking product->user city; apply only if requested
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
                    ImageUrl = p.ImageUrl,
                    Rating = p.Rating,
                    ImageGallery = p.PRODUCT_IMAGEs
                        .OrderBy(i => i.SortOrder)
                        .Select(i => i.Url!)
                        .ToList()
                })
                .ToList();

            // if Url can be null, filter nulls after query
            foreach (var p in products)
                p.ImageGallery = p.ImageGallery?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            var items = products;
            return Ok(items);
        }

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
                    ImageUrl = p.ImageUrl,
                    Rating = p.Rating,
                    ImageGallery = p.PRODUCT_IMAGEs
                        .OrderBy(i => i.SortOrder)
                        .Select(i => i.Url!)
                        .ToList()
                })
                .ToList();

            return Ok(items);
        }

        //get by id
        [HttpGet("{id}")]
        public ActionResult getbyid(int id)
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
                ImageUrl = b.ImageUrl,
                ImageGallery = db.PRODUCT_IMAGEs
                    .AsNoTracking()
                    .Where(i => i.Pid == b.Pid)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.Url)
                    .Where(u => u != null && u != "")
                    .ToList()!,
                Rating = b.Rating
            });
        }

        //Edit

        //DELETE
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var uid = UserClaims.RequireUid(User);

            PRODUCT? b = db.PRODUCTs.Find(id);
            if (b == null) return NotFound();
            if (b.Uid != uid) return Forbid();
            db.PRODUCTs.Remove(b);
            db.SaveChanges();
            return Ok(new { id = b.Pid, deleted = true });


        }
        // add
        [HttpPost]
        public ActionResult post(ProductRequestDto b)
        {
            var uid = UserClaims.RequireUid(User);

            if (b == null) return BadRequest("products is null");
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
                ImageUrl = b.ImageUrl,
                Rating = b.Rating
            };
            db.PRODUCTs.Add(entity);
            db.SaveChanges();

            if (b.ImageGallery is { Count: > 0 })
            {
                var images = b.ImageGallery
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Select((u, idx) => new PRODUCT_IMAGE { Pid = entity.Pid, Url = u, SortOrder = idx })
                    .ToList();
                db.PRODUCT_IMAGEs.AddRange(images);
                db.SaveChanges();
            }

            return CreatedAtAction(nameof(getbyid), new { id = entity.Pid }, new ProductResponseDto
            {
                Pid = entity.Pid,
                Description = entity.Description,
                Price = entity.Price,
                Added_date = entity.Added_date,
                Quantity = entity.Quantity,
                Uid = entity.Uid,
                Cid = entity.Cid,
                Category = entity.Category,
                ImageUrl = entity.ImageUrl,
                ImageGallery = b.ImageGallery,
                Rating = entity.Rating
            });


        }
        [HttpPut("{id}")]
        public ActionResult edit(ProductRequestDto b, int id)
        {
            var uid = UserClaims.RequireUid(User);

            if (b == null) return BadRequest("products is null");
            var entity = db.PRODUCTs.Find(id);
            if (entity == null) return NotFound();
            if (entity.Uid != uid) return Forbid();
            entity.Description = b.Description;
            entity.Price = b.Price;
            entity.Added_date = b.Added_date;
            entity.Quantity = b.Quantity;
            entity.Cid = b.Cid;
            entity.Category = b.Category;
            entity.ImageUrl = b.ImageUrl;
            entity.Rating = b.Rating;

            // Replace gallery
            var existingImages = db.PRODUCT_IMAGEs.Where(i => i.Pid == entity.Pid).ToList();
            if (existingImages.Count > 0)
                db.PRODUCT_IMAGEs.RemoveRange(existingImages);

            if (b.ImageGallery is { Count: > 0 })
            {
                var images = b.ImageGallery
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Select((u, idx) => new PRODUCT_IMAGE { Pid = entity.Pid, Url = u, SortOrder = idx })
                    .ToList();
                db.PRODUCT_IMAGEs.AddRange(images);
            }

            db.SaveChanges();
            return NoContent();

        }
    }
}
