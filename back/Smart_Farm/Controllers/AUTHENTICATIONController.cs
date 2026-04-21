using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Farm.DTOS;
using Smart_Farm.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Smart_Farm.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(
    farContext db,
    IConfiguration config,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole<int>> roleManager) : ControllerBase
{
    private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDTO dto, CancellationToken cancellationToken)
    {
        var identityExists = await userManager.FindByEmailAsync(dto.Email);
        if (identityExists is not null)
            return Conflict("Email is already registered.");

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var domainUser = new USER
        {
            First_name = dto.First_name,
            Last_name = dto.Last_name,
            Email = dto.Email,
            Address_line = dto.Address_line,
            City_name = dto.City_name,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Role = dto.Role,
            PasswordHashed = "IDENTITY_MANAGED"
        };

        db.USERs.Add(domainUser);
        await db.SaveChangesAsync(cancellationToken);

        var phone = new USER_PHONE
        {
            Uid = domainUser.Uid,
            Phone = dto.Phone
        };

        db.USER_PHONEs.Add(phone);
        await db.SaveChangesAsync(cancellationToken);

        var appUser = new AppUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            DomainUserId = domainUser.Uid,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(appUser, dto.Password);
        if (!createResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(createResult.Errors.Select(e => e.Description));
        }

        if (!await roleManager.RoleExistsAsync(dto.Role))
        {
            var roleCreation = await roleManager.CreateAsync(new IdentityRole<int>(dto.Role));
            if (!roleCreation.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return BadRequest(roleCreation.Errors.Select(e => e.Description));
            }
        }

        var addRoleResult = await userManager.AddToRoleAsync(appUser, dto.Role);
        if (!addRoleResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(addRoleResult.Errors.Select(e => e.Description));
        }

        await transaction.CommitAsync(cancellationToken);
        return Ok("User Registered Successfully");
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO dto, CancellationToken cancellationToken)
    {
        var appUser = await userManager.FindByEmailAsync(dto.Email);
        if (appUser is null)
            return Unauthorized("Invalid Email or Password");

        var validPassword = await userManager.CheckPasswordAsync(appUser, dto.Password);
        if (!validPassword)
            return Unauthorized("Invalid Email or Password");

        var roleNames = await userManager.GetRolesAsync(appUser);
        var mainRole = roleNames.FirstOrDefault() ?? "User";
        var domainUser = appUser.DomainUserId is null
            ? null
            : await db.USERs.FirstOrDefaultAsync(u => u.Uid == appUser.DomainUserId, cancellationToken);

        var keyValue = _config["Jwt:Key"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("uid", (appUser.DomainUserId ?? 0).ToString()),
            new Claim("email", appUser.Email ?? dto.Email),
            new Claim("role", mainRole)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            Uid = appUser.DomainUserId,
            First_name = domainUser?.First_name,
            Last_name = domainUser?.Last_name,
            Email = appUser.Email,
            Role = mainRole
        });
    }
}
