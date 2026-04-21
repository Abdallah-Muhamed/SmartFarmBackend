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

        public userController(farContext db, UserManager<AppUser> userManager)
        {
            this.db = db;
            _userManager = userManager;
        }

        // me (alias: GET api/user and GET api/user/me)
        [HttpGet]
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var uid = UserClaims.RequireUid(User);
            var user = GetUserDtoById(uid);
            return user is null ? NotFound() : Ok(user);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe(UserUpdateDto b, CancellationToken cancellationToken)
        {
            var uid = UserClaims.RequireUid(User);

            // Update domain user
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

            // Update identity user (email/username) if linked
            var appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.DomainUserId == uid, cancellationToken);
            if (appUser is not null)
            {
                if (!string.IsNullOrWhiteSpace(b.Email) && !string.Equals(appUser.Email, b.Email, StringComparison.OrdinalIgnoreCase))
                {
                    appUser.Email = b.Email;
                    appUser.UserName = b.Email;
                    appUser.NormalizedEmail = b.Email.ToUpperInvariant();
                    appUser.NormalizedUserName = b.Email.ToUpperInvariant();
                }
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
            return NoContent();
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMe(CancellationToken cancellationToken)
        {
            var uid = UserClaims.RequireUid(User);

            // Delete identity + domain user together when possible
            var appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.DomainUserId == uid, cancellationToken);

            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var domain = await db.USERs.FindAsync([uid], cancellationToken);
            if (domain is null) return NotFound();

            db.USERs.Remove(domain);
            await db.SaveChangesAsync(cancellationToken);

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

        // my crops
        [HttpGet("me/crops")]
        public IActionResult GetMyCrops()
        {
            var uid = UserClaims.RequireUid(User);
            var crops = db.CROPs
                .Where(c => c.Uid == uid)
                .Select(c => new
                {
                   
                    UserName = c.UidNavigation.First_name + " " + c.UidNavigation.Last_name,

                     
                    c.Cid,
                    c.Notes,
                    c.Area_size,
                    c.Start_date,
                    c.Soil_type,
                    c.Current_Stage,
                    
                })
                .ToList();

            return Ok(crops);
        }

        // my orders
        [HttpGet("me/orders")]
        public IActionResult GetMyOrders()
        {
            var uid = UserClaims.RequireUid(User);
            var crops = db.ORDERs
                .Where(c => c.Uid == uid)
                .Select(c => new
                {
                   
                    UserName = c.UidNavigation.First_name + " " + c.UidNavigation.Last_name,

                   
                    c.Oid,
                    c.Status,
                    c.Order_date,
                    c.Quantity,
                    c.Total_price,
                    c.Pid,
                   
                    

                })
                .ToList();

            return Ok(crops);
        }

        // my products
        [HttpGet("me/products")]
        public IActionResult GetMyProducts()
        {
            var uid = UserClaims.RequireUid(User);
            var crops = db.PRODUCTs
                .Where(c => c.Uid == uid)
                .Select(c => new
                {
                    // اسم المستخدم
                    UserName = c.UidNavigation.First_name + " " + c.UidNavigation.Last_name,

                    // بيانات المحصول
                    c.Pid,
                    c.Description,
                    c.Price,
                    c.Added_date,
                    c.Quantity,
                    c.Cid,

                })
                .ToList();

            return Ok(crops);
        }
        
        // my tasks
        [HttpGet("me/tasks")]
        public IActionResult GetMyTasks()
        {
            var uid = UserClaims.RequireUid(User);
            var crops = db.Tasks
                .Where(c => c.Uid == uid)
                .Select(c => new
                {
                   
                    UserName = c.UidNavigation.First_name + " " + c.UidNavigation.Last_name,

                  
                    c.Task_id,
                    c.Date,
                    c.Label,
                    c.Content,
                    c.State,
                    c.Uid,

                })
                .ToList();

            return Ok(crops);
        }
        // my phones
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
        //[HttpGet("{id}/phones")]
        //public IActionResult GetUserPhone(int id)
        //{
        //    var phones = db.USER_PHONEs
        //        .Where(p => p.Uid == id)
        //        .Select(p => p.Phone)
        //        .ToList();

        //    return Ok(phones);
        //}


        // NOTE: user updates/deletes should go through PUT/DELETE api/user/me

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
                    Phones = u.USER_PHONEs.Select(p => p.Phone).ToList()
                })
                .FirstOrDefault();
        }
    }
}
