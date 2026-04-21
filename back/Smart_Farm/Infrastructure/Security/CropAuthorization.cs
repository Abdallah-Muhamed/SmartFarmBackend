using Microsoft.AspNetCore.Mvc;
using Smart_Farm.Models;

namespace Smart_Farm.Infrastructure.Security;

public static class CropAuthorization
{
    public static ActionResult? EnsureCropOwnedByUser(farContext db, int cid, int uid)
    {
        var crop = db.CROPs.Find(cid);
        if (crop is null)
            return new NotFoundObjectResult(new { message = "Crop not found." });

        if (crop.Uid != uid)
            return new ForbidResult();

        return null;
    }
}

