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
public class OrderController(farContext db) : ControllerBase
{
    private readonly farContext _db = db;

    private static IQueryable<OrderDTO> ProjectOrders(IQueryable<ORDER> orders) =>
        orders.Select(o => new OrderDTO
        {
            Oid = o.Oid,
            Status = o.Status ?? "pending",
            Order_date = o.Order_date,
            Quantity = o.Quantity,
            Total_price = o.Total_price,
            Pid = o.Pid,
            Uid = o.Uid,
            UserName = o.UidNavigation != null
                ? ((o.UidNavigation.First_name ?? "") + " " + (o.UidNavigation.Last_name ?? "")).Trim()
                : "مستخدم",
            BuyerName = o.UidNavigation != null
                ? ((o.UidNavigation.First_name ?? "") + " " + (o.UidNavigation.Last_name ?? "")).Trim()
                : null,
            ProductName = o.PidNavigation != null
                ? (o.PidNavigation.Description ?? "منتج")
                : "منتج محذوف",
            // Read seller info from USER table via SellerUid
            SellerName = o.SellerUidNavigation != null
                ? ((o.SellerUidNavigation.First_name ?? "") + " " + (o.SellerUidNavigation.Last_name ?? "")).Trim()
                : null,
            SellerPhone = o.SellerUidNavigation != null
                ? o.SellerUidNavigation.USER_PHONEs.Select(p => p.Phone).FirstOrDefault()
                : null,
            SellerAddress = o.SellerUidNavigation != null ? o.SellerUidNavigation.Address_line : null,
            SellerCity = o.SellerUidNavigation != null ? o.SellerUidNavigation.City_name : null,
            // Read buyer info from USER table via Uid
            BuyerPhone = o.UidNavigation != null
                ? o.UidNavigation.USER_PHONEs.Select(p => p.Phone).FirstOrDefault()
                : null,
            BuyerAddress = o.UidNavigation != null ? o.UidNavigation.Address_line : null,
            BuyerCity = o.UidNavigation != null ? o.UidNavigation.City_name : null,
            Payment_method = o.Payment_method,
            Promo_code = o.Promo_code,
            Discount_amount = o.Discount_amount,
            Order_notes = o.Order_notes
        });

    [HttpGet]
    [HttpGet("me")]
    public async Task<ActionResult> GetMine()
    {
        var uid = UserClaims.RequireUid(User);

        var orders = await ProjectOrders(
                _db.ORDERs
                    .AsNoTracking()
                    .Where(o => o.Uid == uid || (o.PidNavigation != null && o.PidNavigation.Uid == uid)))
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        var uid = UserClaims.RequireUid(User);

        var order = await ProjectOrders(
                _db.ORDERs
                    .AsNoTracking()
                    .Where(o => o.Oid == id && (o.Uid == uid || (o.PidNavigation != null && o.PidNavigation.Uid == uid))))
            .FirstOrDefaultAsync();

        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult> Create(OrderRequestDto dto)
    {
        var uid = UserClaims.RequireUid(User);

        // Fetch product to get seller UID
        var product = await _db.PRODUCTs
            .FirstOrDefaultAsync(p => p.Pid == dto.Pid);

        if (product == null)
            return BadRequest("Product not found");

        // Check if enough quantity is available
        if (product.Quantity < dto.Quantity)
            return BadRequest("Not enough quantity available");

        // Deduct quantity from product
        product.Quantity -= dto.Quantity;

        var entity = new ORDER
        {
            Status = dto.Status,
            Order_date = dto.Order_date,
            Quantity = dto.Quantity,
            Total_price = dto.Total_price,
            Pid = dto.Pid,
            Uid = uid,
            SellerUid = product.Uid,
            Payment_method = dto.Payment_method,
            Promo_code = dto.Promo_code,
            Discount_amount = dto.Discount_amount,
            Order_notes = dto.Order_notes,
            CreatedAt = DateTime.UtcNow
        };

        _db.ORDERs.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Oid }, new { entity.Oid });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, OrderRequestDto dto)
    {
        var uid = UserClaims.RequireUid(User);

        var entity = await _db.ORDERs.FirstOrDefaultAsync(o => o.Oid == id);
        if (entity is null)
            return NotFound();

        // Allow only seller (SellerUid) to update the order
        if (entity.SellerUid == null)
            return BadRequest("Order has no seller assigned. Please contact support.");

        if (entity.SellerUid != uid)
            return Forbid();

        // Only update fields that were explicitly provided (PATCH-style)
        if (dto.Status != null)        entity.Status = dto.Status;
        if (dto.Order_date != null)    entity.Order_date = dto.Order_date;
        if (dto.Quantity != null)      entity.Quantity = dto.Quantity;
        if (dto.Total_price != null)   entity.Total_price = dto.Total_price;
        if (dto.Pid != null)           entity.Pid = dto.Pid;
        if (dto.Payment_method != null) entity.Payment_method = dto.Payment_method;
        if (dto.Promo_code != null)    entity.Promo_code = dto.Promo_code;
        if (dto.Discount_amount != null) entity.Discount_amount = dto.Discount_amount;
        if (dto.Order_notes != null)   entity.Order_notes = dto.Order_notes;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("all")]
    public async Task<ActionResult> DeleteAll()
    {
        var uid = UserClaims.RequireUid(User);
        var deletedCount = await _db.ORDERs.Where(o => o.Uid == uid).ExecuteDeleteAsync();
        return Ok(new { deletedCount });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var uid = UserClaims.RequireUid(User);

        var entity = await _db.ORDERs.FirstOrDefaultAsync(o => o.Oid == id);
        if (entity is null)
            return NotFound();

        if (entity.Uid != uid)
            return Forbid();

        _db.ORDERs.Remove(entity);
        await _db.SaveChangesAsync();

        return Ok(new { id, deleted = true });
    }

    [HttpGet("user/{uid:int}")]
    public async Task<ActionResult> GetByUser(int uid)
    {
        var me = UserClaims.RequireUid(User);

        if (uid != me)
            return Forbid();

        var orders = await ProjectOrders(
                _db.ORDERs
                    .AsNoTracking()
                    .Where(o => o.Uid == uid))
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("product/{pid:int}")]
    public async Task<ActionResult> GetByProduct(int pid)
    {
        var uid = UserClaims.RequireUid(User);

        var orders = await ProjectOrders(
                _db.ORDERs
                    .AsNoTracking()
                    .Where(o => o.Pid == pid && (o.Uid == uid || (o.PidNavigation != null && o.PidNavigation.Uid == uid))))
            .ToListAsync();

        return Ok(orders);
    }

    [HttpPost("batch")]
    public async Task<ActionResult> CreateBatch(BatchOrderRequestDto request)
    {
        var uid = UserClaims.RequireUid(User);

        if (request?.Items == null || request.Items.Count == 0)
            return BadRequest("Items are required.");

        using var transaction = await _db.Database.BeginTransactionAsync();

        var entities = new List<ORDER>();

        foreach (var item in request.Items)
        {
            // Fetch product to get seller UID
            var product = await _db.PRODUCTs
                .FirstOrDefaultAsync(p => p.Pid == item.Pid);

            if (product == null)
                continue; // Skip invalid products

            // Check if enough quantity is available
            if (product.Quantity < item.Quantity)
                continue; // Skip items with insufficient quantity

            // Deduct quantity from product
            product.Quantity -= item.Quantity;

            entities.Add(new ORDER
            {
                Status = item.Status,
                Order_date = item.Order_date,
                Quantity = item.Quantity,
                Total_price = item.Total_price,
                Pid = item.Pid,
                Uid = uid,
                SellerUid = product.Uid,
                Payment_method = request.Payment_method,
                Promo_code = request.Promo_code,
                Discount_amount = request.Discount_amount,
                Order_notes = request.Order_notes,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.ORDERs.AddRangeAsync(entities);
        await _db.SaveChangesAsync();

        await transaction.CommitAsync();

        return Ok(new { created = entities.Count });
    }
}
