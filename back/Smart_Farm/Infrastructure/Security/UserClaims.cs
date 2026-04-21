using System.Security.Claims;

namespace Smart_Farm.Infrastructure.Security;

public static class UserClaims
{
    public static bool TryGetUid(ClaimsPrincipal? user, out int uid)
    {
        uid = default;

        var value = user?.FindFirstValue("uid") ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out uid) && uid > 0;
    }
}

