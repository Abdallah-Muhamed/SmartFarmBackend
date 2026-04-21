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

        //list
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = db.USERs
                .Select(u => new UserDto
                {Longitude=u.Longitude,
                Latitude=u.Latitude,
                    Uid = u.Uid,
                    First_name = u.First_name,
                    Last_name = u.Last_name,
                    Email = u.Email,
                    Address_line = u.Address_line,
                    City_name = u.City_name,
                    Role = u.Role,
                    Phones = u.USER_PHONEs.Select(p => p.Phone).ToList()
                })
                .ToList();

            return Ok(users);
        }

        //get by id

        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = db.USERs
                .Where(u => u.Uid == id)
                .Select(u => new UserDto
                {Latitude=u.Latitude,
                Longitude=u.Longitude,

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

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("me")]
        public IActionResult GetMe()
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            return GetUserById(uid);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe(UserUpdateDto b, CancellationToken cancellationToken)
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

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
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

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

        //get user crops
        [HttpGet("{id}/crops")]
        public IActionResult GetUserCrops(int id)
        {
            var crops = db.CROPs
                .Where(c => c.Uid == id)
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

        [HttpGet("me/crops")]
        public IActionResult GetMyCrops()
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            return GetUserCrops(uid);
        }

        //get user order
        [HttpGet("{id}/orders")]
       
        public IActionResult GetUserorders(int id)
        {
            var crops = db.ORDERs
                .Where(c => c.Uid == id)
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

        [HttpGet("me/orders")]
        public IActionResult GetMyOrders()
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            return GetUserorders(uid);
        }

        //get user products
        [HttpGet("{id}/products")]
        
        public IActionResult GetUserproducts(int id)
        {
            var crops = db.PRODUCTs
                .Where(c => c.Uid == id)
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

        [HttpGet("me/products")]
        public IActionResult GetMyProducts()
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            return GetUserproducts(uid);
        }
        //get user tasks
        [HttpGet("{id}/tasks")]
        
        public IActionResult GetUsertasks(int id)
        {
            var crops = db.Tasks
                .Where(c => c.Uid == id)
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

        [HttpGet("me/tasks")]
        public IActionResult GetMyTasks()
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            return GetUsertasks(uid);
        }
        //users phones only

        [HttpGet("{id}/phones")]
        public IActionResult GetUserPhones(int id)
        {
            var phones = db.USER_PHONEs
                .Where(p => p.Uid == id)
                .Select(p => new
                {

                    UserName = p.UidNavigation.First_name + " " + p.UidNavigation.Last_name,
                  p.Phone

                })
                .ToList();

            return Ok(phones);
        }

        [HttpGet("me/phones")]
        public IActionResult GetMyPhones()
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            return GetUserPhones(uid);
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


        //Edit
        [HttpPut("{id}")]
        public IActionResult Edit(UserUpdateDto b, int id)
        {
            if (b == null) return BadRequest("Users is null");
            var entity = db.USERs.Find(id);
            if (entity == null) return NotFound();
            entity.First_name = b.First_name;
            entity.Last_name = b.Last_name;
            entity.Email = b.Email;
            entity.Address_line = b.Address_line;
            entity.City_name = b.City_name;
            entity.Latitude = b.Latitude;
            entity.Longitude = b.Longitude;
            entity.Role = b.Role;
            db.SaveChanges();
            return NoContent();
        }
        //DELETE
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            USER? b = db.USERs.Find(id);
            if (b == null) return NotFound();
            db.USERs.Remove(b);
            db.SaveChanges();
            return Ok(new { id = b.Uid, deleted = true });


        }
    }
}
