using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Smart_Farm.DTOS;
using Smart_Farm.Infrastructure.Security;
using Smart_Farm.Models;

namespace Smart_Farm.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class userController : ControllerBase
    {
        farContext db;
        private readonly UserManager<AppUser> _userManager;
        private readonly Cloudinary _cloudinary;

        public userController(farContext db, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            this.db = db;
            _userManager = userManager;

            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];
            _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
        }

        // ─── GET api/user  /  GET api/user/me ───────────────────────────────────
        [HttpGet]
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var uid = UserClaims.RequireUid(User);
            var user = GetUserDtoById(uid);
            return user is null ? NotFound() : Ok(user);
        }

        // ─── PUT api/user/me ─────────────────────────────────────────────────────
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe(UserUpdateDto b, CancellationToken cancellationToken)
        {
            var uid = UserClaims.RequireUid(User);
            var domain = await db.USERs.FindAsync([uid], cancellationToken);
            if (domain is null) return NotFound();

            domain.First_name = b.First_name;
            domain.Last_name = b.Last_name;
            domain.Email = b.Email;
            domain.Address_line = b.Address_line;
            domain.City_name = b.City_name;
            domain.Latitude = b.Latitude;
            domain.Longitude = b.Longitude;
            domain.Role = b.Role;

            if (b.Phones is not null)
            {
                var existingPhones = await db.USER_PHONEs
                    .Where(p => p.Uid == uid)
                    .ToListAsync(cancellationToken);

                db.USER_PHONEs.RemoveRange(existingPhones);

                foreach (var phone in b.Phones.Where(p => !string.IsNullOrWhiteSpace(p)))
                    db.USER_PHONEs.Add(new USER_PHONE { Uid = uid, Phone = phone.Trim() });
            }

            var appUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.DomainUserId == uid, cancellationToken);

            if (appUser is not null &&
                !string.IsNullOrWhiteSpace(b.Email) &&
                !string.Equals(appUser.Email, b.Email, StringComparison.OrdinalIgnoreCase))
            {
                appUser.Email = b.Email;
                appUser.UserName = b.Email;
                appUser.NormalizedEmail = b.Email.ToUpperInvariant();
                appUser.NormalizedUserName = b.Email.ToUpperInvariant();
            }

            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            if (appUser is not null)
            {
                var identityUpdate = await _userManager.UpdateAsync(appUser);
                if (!identityUpdate.Succeeded)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return BadRequest(identityUpdate.Errors.Select(e => e.Description));
                }
            }

            await tx.CommitAsync(cancellationToken);
            return Ok("User information has been updated");
        }

        // ─── POST api/user/me/Profile_Photo ──────────────────────────────────────────────
        [HttpPost("me/Profile_Photo")]
        public async Task<IActionResult> UploadPhoto(IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
                return BadRequest("No file provided.");

            var allowed = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowed.Contains(file.ContentType.ToLower()))
                return BadRequest("Only image files (jpg, png, webp, gif) are allowed.");

            var uid = UserClaims.RequireUid(User);
            var domain = await db.USERs.FindAsync([uid], cancellationToken);
            if (domain is null) return NotFound();

            // Delete old photo from Cloudinary if exists
            if (!string.IsNullOrWhiteSpace(domain.PhotoUrl))
            {
                var oldPublicId = ExtractPublicId(domain.PhotoUrl);
                if (!string.IsNullOrWhiteSpace(oldPublicId))
                    await _cloudinary.DestroyAsync(new DeletionParams(oldPublicId));
            }

            // Upload new photo
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "smart_farm/users",
                PublicId = $"user_{uid}",
                Overwrite = true,
                Transformation = new Transformation().Width(400).Height(400).Crop("fill").Gravity("face")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error is not null)
                return StatusCode(500, uploadResult.Error.Message);

            domain.PhotoUrl = uploadResult.SecureUrl.ToString();
            await db.SaveChangesAsync(cancellationToken);

            return Ok(new { photoUrl = domain.PhotoUrl });
        }

        // ─── DELETE api/user/me/photo ────────────────────────────────────────────
        [HttpDelete("me/Profile_Photo")]
        public async Task<IActionResult> DeletePhoto(CancellationToken cancellationToken)
        {
            var uid = UserClaims.RequireUid(User);
            var domain = await db.USERs.FindAsync([uid], cancellationToken);
            if (domain is null) return NotFound();

            if (string.IsNullOrWhiteSpace(domain.PhotoUrl))
                return BadRequest("No photo to delete.");

            var publicId = ExtractPublicId(domain.PhotoUrl);
            if (!string.IsNullOrWhiteSpace(publicId))
                await _cloudinary.DestroyAsync(new DeletionParams(publicId));

            domain.PhotoUrl = null;
            await db.SaveChangesAsync(cancellationToken);

            return Ok(new { deleted = true });
        }

        // ─── DELETE api/user/me ──────────────────────────────────────────────────
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMe(CancellationToken cancellationToken)
        {
            var uid = UserClaims.RequireUid(User);
            var appUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.DomainUserId == uid, cancellationToken);

            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

            var domain = await db.USERs.FindAsync([uid], cancellationToken);
            if (domain is null) return NotFound();

            // 1. Delete photo from Cloudinary
            if (!string.IsNullOrWhiteSpace(domain.PhotoUrl))
            {
                var publicId = ExtractPublicId(domain.PhotoUrl);
                if (!string.IsNullOrWhiteSpace(publicId))
                    await _cloudinary.DestroyAsync(new DeletionParams(publicId));
            }

            // 2. Delete all related data first (to avoid FK violations)
            var phones = db.USER_PHONEs.Where(p => p.Uid == uid);
            db.USER_PHONEs.RemoveRange(phones);

            var orders = db.ORDERs.Where(o => o.Uid == uid);
            db.ORDERs.RemoveRange(orders);

            var tasks = db.Tasks.Where(t => t.Uid == uid);
            db.Tasks.RemoveRange(tasks);

            var crops = db.CROPs.Where(c => c.Uid == uid);
            db.CROPs.RemoveRange(crops);

            var products = db.PRODUCTs.Where(p => p.Uid == uid);
            db.PRODUCTs.RemoveRange(products);

            // 3. Delete the domain user
            db.USERs.Remove(domain);
            await db.SaveChangesAsync(cancellationToken);

            // 4. Delete identity user
            if (appUser is not null)
            {
                var identityDelete = await _userManager.DeleteAsync(appUser);
                if (!identityDelete.Succeeded)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return BadRequest(identityDelete.Errors.Select(e => e.Description));
                }
            }

            await tx.CommitAsync(cancellationToken);
            return Ok(new { id = uid, deleted = true });
        }

        // ─── GET api/user/me/phones ──────────────────────────────────────────────
        [HttpGet("me/phones")]
        public IActionResult GetMyPhones()
        {
            var uid = UserClaims.RequireUid(User);
            var phones = db.USER_PHONEs
                .Where(p => p.Uid == uid)
                .Select(p => new
                {
                    UserName = p.UidNavigation.First_name + " " + p.UidNavigation.Last_name,
                    p.Phone
                })
                .ToList();
            return Ok(phones);
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────
        private UserDto? GetUserDtoById(int id)
        {
            return db.USERs
                .Where(u => u.Uid == id)
                .Select(u => new UserDto
                {
                    Latitude = u.Latitude,
                    Longitude = u.Longitude,
                    Uid = u.Uid,
                    First_name = u.First_name,
                    Last_name = u.Last_name,
                    Email = u.Email,
                    Address_line = u.Address_line,
                    City_name = u.City_name,
                    Role = u.Role,
                    PhotoUrl = u.PhotoUrl,
                    Phones = u.USER_PHONEs.Select(p => p.Phone).ToList()
                })
                .FirstOrDefault();
        }

        /// <summary>
        /// Extracts the Cloudinary public_id from a secure URL.
        /// e.g. https://res.cloudinary.com/dot312kut/image/upload/v123/smart_farm/users/user_5.jpg
        ///      → smart_farm/users/user_5
        /// </summary>
        private static string? ExtractPublicId(string url)
        {
            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/');
                // find "upload" segment index
                var uploadIdx = Array.IndexOf(segments, "upload");
                if (uploadIdx < 0) return null;

                // skip version segment (v12345) if present
                var start = uploadIdx + 1;
                if (start < segments.Length && segments[start].StartsWith('v') &&
                    long.TryParse(segments[start][1..], out _))
                    start++;

                var publicIdWithExt = string.Join("/", segments[start..]);
                // remove extension
                var dot = publicIdWithExt.LastIndexOf('.');
                return dot >= 0 ? publicIdWithExt[..dot] : publicIdWithExt;
            }
            catch { return null; }
        }
    }
}