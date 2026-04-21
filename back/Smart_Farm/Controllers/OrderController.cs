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

        // list mine (alias: GET api/order and GET api/order/me)
        [HttpGet]
        [HttpGet("me")]
        public IActionResult GetMine()
        {
            var uid = UserClaims.RequireUid(User);

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
            var uid = UserClaims.RequireUid(User);
            var order = db.ORDERs
                .Include(o => o.UidNavigation)
                .Include(o => o.PidNavigation)
                .Where(o => o.Oid == id && o.Uid == uid)
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
            var uid = UserClaims.RequireUid(User);

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
            var uid = UserClaims.RequireUid(User);

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
            var uid = UserClaims.RequireUid(User);

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
            var me = UserClaims.RequireUid(User);
            if (uid != me) return Forbid();

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
            var uid = UserClaims.RequireUid(User);
            var orders = db.ORDERs
                .Include(o => o.UidNavigation)
                .Include(o => o.PidNavigation)
                .Where(o => o.Pid == pid && o.Uid == uid)
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
            var uid = UserClaims.RequireUid(User);

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

              
            
        
    



