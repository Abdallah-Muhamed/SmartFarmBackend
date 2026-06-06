using Microsoft.EntityFrameworkCore;
using Smart_Farm.Models;

namespace Smart_Farm.Infrastructure.Persistence;

public static class ProductDeletionHelper
{
    public static async System.Threading.Tasks.Task RemoveDependenciesAsync(
        farContext db, IReadOnlyList<int> productIds, CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
            return;

        await db.ORDERs
            .Where(o => o.Pid != null && productIds.Contains(o.Pid.Value))
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.Pid, (int?)null), cancellationToken);

        await db.REVIEWs
            .Where(r => r.Pid != null && productIds.Contains(r.Pid.Value))
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Pid, (int?)null), cancellationToken);
    }
}
