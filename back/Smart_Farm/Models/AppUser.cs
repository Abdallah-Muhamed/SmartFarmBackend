using Microsoft.AspNetCore.Identity;

namespace Smart_Farm.Models;

public class AppUser : IdentityUser<int>
{
    public int? DomainUserId { get; set; }
}
