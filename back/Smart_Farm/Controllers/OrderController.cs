using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class OrderController : ControllerBase
    {



        farContext db;

        public OrderController(farContext context)
        {
            db = context;
        }

        // get all
        [HttpGet]
        public IActionResult GetAll()
        {
            var orders = db.ORDERs
                .Include(o => o.UidNavigation)
                .Include(o => o.PidNavigation)
                .Select(o => new OrderDTO
                {
                    Oid = o.Oid,
                    Status = o.Status,
                    Order_date = o.Order_date,
                    Quantity = o.Quantity,
                    Total_price = o.Total_price,
                    Uid = o.Uid,
                    Pid = o.Pid,
                    UserName = o.UidNavigation.First_name,
                    ProductName = o.PidNavigation.Description
                })
                .ToList();

            return Ok(orders);
        }

        [HttpGet("me")]
        public IActionResult GetMine()
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            var orders = db.ORDERs
                .Include(o => o.UidNavigation)
                .Include(o => o.PidNavigation)
                .Where(o => o.Uid == uid)
                .Select(o => new OrderDTO
                {
                    Oid = o.Oid,
                    Status = o.Status,
                    Order_date = o.Order_date,
                    Quantity = o.Quantity,
                    Total_price = o.Total_price,
                    Uid = o.Uid,
                    Pid = o.Pid,
                    UserName = o.UidNavigation.First_name,
                    ProductName = o.PidNavigation.Description
                })
                .ToList();

            return Ok(orders);
        }

        // get by id
        [HttpGet("{id}")]
        public ActionResult GetById(int id)
        {
            var order = db.ORDERs
                .Include(o => o.UidNavigation)
                .Include(o => o.PidNavigation)
                .Where(o => o.Oid == id)
                .Select(o => new OrderDTO
                {
                    Oid = o.Oid,
                    Status = o.Status,
                    Order_date = o.Order_date,
                    Quantity = o.Quantity,
                    Total_price = o.Total_price,
                    Uid = o.Uid,
                    Pid = o.Pid,
                    UserName = o.UidNavigation.First_name,
                    ProductName = o.PidNavigation.Description
                })
                .FirstOrDefault();

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        // add
        [HttpPost]
        public ActionResult post(OrderRequestDto b)
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            if (b == null) return BadRequest("orders is null");
            if (!ModelState.IsValid) return BadRequest();
            var entity = new ORDER
            {
                Status = b.Status,
                Order_date = b.Order_date,
                Quantity = b.Quantity,
                Total_price = b.Total_price,
                Pid = b.Pid,
                Uid = uid,
                Payment_method = b.Payment_method,
                Promo_code = b.Promo_code,
                Discount_amount = b.Discount_amount,
                Order_notes = b.Order_notes
            };
            db.ORDERs.Add(entity);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = entity.Oid }, new { entity.Oid });


        }



        // edit
        [HttpPut("{id}")]

        public ActionResult edit(OrderRequestDto b, int id)
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            if (b == null) return BadRequest("orders is null");
            var entity = db.ORDERs.Find(id);
            if (entity == null) return NotFound();
            if (entity.Uid != uid) return Forbid();
            entity.Status = b.Status;
            entity.Order_date = b.Order_date;
            entity.Quantity = b.Quantity;
            entity.Total_price = b.Total_price;
            entity.Pid = b.Pid;
            entity.Payment_method = b.Payment_method;
            entity.Promo_code = b.Promo_code;
            entity.Discount_amount = b.Discount_amount;
            entity.Order_notes = b.Order_notes;
            db.SaveChanges();
            return NoContent();

        }
        // delete
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            ORDER? b = db.ORDERs.Find(id);
            if (b == null) return NotFound();
            if (b.Uid != uid) return Forbid();
            db.ORDERs.Remove(b);
            db.SaveChanges();
            return Ok(new { id = b.Oid, deleted = true });

        }

        // get all orders to specific user=id

        [HttpGet("user/{uid}")]
        public IActionResult GetOrdersByUser(int uid)
        {
            var orders = db.ORDERs
                .Include(o => o.UidNavigation)
                .Include(o => o.PidNavigation)
                .Where(o => o.Uid == uid)
                .Select(o => new OrderDTO
                {
                    Oid = o.Oid,
                    Status = o.Status,
                    Order_date = o.Order_date,
                    Quantity = o.Quantity,
                    Total_price = o.Total_price,
                    Uid = o.Uid,
                    Pid = o.Pid,
                    UserName = o.UidNavigation.First_name,
                    ProductName = o.PidNavigation.Description
                })
                .ToList();

            return Ok(orders);
        }


        //get all orders to specefic product
        [HttpGet("product/{pid}")]
        public ActionResult GetOrdersByProduct(int pid)
        {
            var orders = db.ORDERs
                .Include(o => o.UidNavigation)
                .Include(o => o.PidNavigation)
                .Where(o => o.Pid == pid)
                .Select(o => new OrderDTO
                {
                    Oid = o.Oid,
                    Status = o.Status,
                    Order_date = o.Order_date,
                    Quantity = o.Quantity,
                    Total_price = o.Total_price,
                    Uid = o.Uid,
                    Pid = o.Pid,
                    UserName = o.UidNavigation.First_name,
                    ProductName = o.PidNavigation.Description
                })
                .ToList();

            return Ok(orders);
        }

        [HttpPost("batch")]
        public IActionResult CreateBatch(BatchOrderRequestDto request)
        {
            if (!UserClaims.TryGetUid(User, out var uid))
                return Unauthorized();

            var items = request?.Items;
            if (items is null || items.Count == 0)
                return BadRequest("items is required.");

            var entities = items.Select(i => new ORDER
            {
                Status = i.Status,
                Order_date = i.Order_date,
                Quantity = i.Quantity,
                Total_price = i.Total_price,
                Pid = i.Pid,
                Uid = uid,
                Payment_method = request?.Payment_method,
                Promo_code = request?.Promo_code,
                Discount_amount = request?.Discount_amount,
                Order_notes = request?.Order_notes
            }).ToList();

            db.ORDERs.AddRange(entities);
            db.SaveChanges();
            return Ok(new { created = entities.Count });
        }

    }
}

              
            
        
    



