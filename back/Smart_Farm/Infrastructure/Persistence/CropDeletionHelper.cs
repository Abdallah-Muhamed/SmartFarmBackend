using Microsoft.EntityFrameworkCore;
using Smart_Farm.Models;

namespace Smart_Farm.Infrastructure.Persistence;

public static class CropDeletionHelper
{
    public static async System.Threading.Tasks.Task RemoveDependenciesAsync(
        farContext db, IReadOnlyList<int> cropIds, CancellationToken cancellationToken)
    {
        if (cropIds.Count == 0)
            return;

        await db.IRRIGATIONs
            .Where(i => i.Cid != null && cropIds.Contains(i.Cid.Value))
            .ExecuteDeleteAsync(cancellationToken);

        await db.IRRIGATION_STAGEs
            .Where(s => s.Cid != null && cropIds.Contains(s.Cid.Value))
            .ExecuteDeleteAsync(cancellationToken);

        await db.CROP_WATER_BALANCE_LOGs
            .Where(l => cropIds.Contains(l.Cid))
            .ExecuteDeleteAsync(cancellationToken);

        await db.AI_Diagnoses
            .Where(d => d.Cid != null && cropIds.Contains(d.Cid.Value))
            .ExecuteUpdateAsync(
                s => s.SetProperty(d => d.Cid, (int?)null).SetProperty(d => d.FarmId, (int?)null),
                cancellationToken);

        await db.PRODUCTs
            .Where(p => p.Cid != null && cropIds.Contains(p.Cid.Value))
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Cid, (int?)null), cancellationToken);

        await db.Tasks
            .Where(t => t.Cid != null && cropIds.Contains(t.Cid.Value))
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Cid, (int?)null), cancellationToken);

        var crops = await db.CROPs
            .Where(c => cropIds.Contains(c.Cid))
            .Include(c => c.Frs)
            .ToListAsync(cancellationToken);

        foreach (var crop in crops)
            crop.Frs.Clear();

        if (crops.Count > 0)
            await db.SaveChangesAsync(cancellationToken);
    }
}
