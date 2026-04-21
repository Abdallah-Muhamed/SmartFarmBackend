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

    public static int RequireUid(ClaimsPrincipal? user)
    {
        if (TryGetUid(user, out var uid))
            return uid;

        throw new UnauthorizedAccessException("Missing or invalid uid claim.");
    }
}

