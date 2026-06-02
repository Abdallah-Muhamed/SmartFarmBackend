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

    // ??????????????? Helper Mapper ???????????????
    private static OrderDTO Map(ORDER o) => new()
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
    };

    // ??????????????? GET mine ???????????????
    [HttpGet]
    [HttpGet("me")]
    public async Task<ActionResult> GetMine()
    {
        var uid = UserClaims.RequireUid(User);

        var orders = await _db.ORDERs
            .AsNoTracking()
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
            .ToListAsync();

        return Ok(orders);
    }

    // ??????????????? GET by id ???????????????
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        var uid = UserClaims.RequireUid(User);

        var order = await _db.ORDERs
            .AsNoTracking()
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
            .FirstOrDefaultAsync();

        return order is null ? NotFound() : Ok(order);
    }

    // ??????????????? POST ???????????????
    [HttpPost]
    public async Task<ActionResult> Create(OrderRequestDto dto)
    {
        var uid = UserClaims.RequireUid(User);

        var entity = new ORDER
        {
            Status = dto.Status,
            Order_date = dto.Order_date,
            Quantity = dto.Quantity,
            Total_price = dto.Total_price,
            Pid = dto.Pid,
            Uid = uid,
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

    // ??????????????? PUT ???????????????
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, OrderRequestDto dto)
    {
        var uid = UserClaims.RequireUid(User);

        var entity = await _db.ORDERs.FirstOrDefaultAsync(o => o.Oid == id);

        if (entity is null)
            return NotFound();

        if (entity.Uid != uid)
            return Forbid();

        entity.Status = dto.Status;
        entity.Order_date = dto.Order_date;
        entity.Quantity = dto.Quantity;
        entity.Total_price = dto.Total_price;
        entity.Pid = dto.Pid;
        entity.Payment_method = dto.Payment_method;
        entity.Promo_code = dto.Promo_code;
        entity.Discount_amount = dto.Discount_amount;
        entity.Order_notes = dto.Order_notes;

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

    // ??????????????? DELETE ???????????????
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

    // ??????????????? GET by user (admin/self safe) ???????????????
    [HttpGet("user/{uid:int}")]
    public async Task<ActionResult> GetByUser(int uid)
    {
        var me = UserClaims.RequireUid(User);

        if (uid != me)
            return Forbid();

        var orders = await _db.ORDERs
            .AsNoTracking()
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
            .ToListAsync();

        return Ok(orders);
    }

    // ??????????????? GET by product ???????????????
    [HttpGet("product/{pid:int}")]
    public async Task<ActionResult> GetByProduct(int pid)
    {
        var uid = UserClaims.RequireUid(User);

        var orders = await _db.ORDERs
            .AsNoTracking()
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
            .ToListAsync();

        return Ok(orders);
    }

    // ??????????????? Batch Create (safe version) ???????????????
    [HttpPost("batch")]
    public async Task<ActionResult> CreateBatch(BatchOrderRequestDto request)
    {
        var uid = UserClaims.RequireUid(User);

        if (request?.Items == null || request.Items.Count == 0)
            return BadRequest("Items are required.");

        using var transaction = await _db.Database.BeginTransactionAsync();

        var entities = request.Items.Select(i => new ORDER
        {
            Status = i.Status,
            Order_date = i.Order_date,
            Quantity = i.Quantity,
            Total_price = i.Total_price,
            Pid = i.Pid,
            Uid = uid,
            Payment_method = request.Payment_method,
            Promo_code = request.Promo_code,
            Discount_amount = request.Discount_amount,
            Order_notes = request.Order_notes,
            CreatedAt = DateTime.UtcNow
        });

        await _db.ORDERs.AddRangeAsync(entities);
        await _db.SaveChangesAsync();

        await transaction.CommitAsync();

        return Ok(new { created = entities.Count() });
    }
}