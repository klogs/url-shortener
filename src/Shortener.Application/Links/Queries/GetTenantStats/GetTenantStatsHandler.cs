using Shortener.Application.Interfaces;

namespace Shortener.Application.Links.Queries.GetTenantStats;

public sealed class GetTenantStatsHandler(ILinkStatsRepository stats)
{
    public async Task<TenantStatsResult> HandleAsync(GetTenantStatsQuery query, CancellationToken ct = default)
    {
        var totalTask = stats.CountTotalAsync(query.TenantId, ct);
        var activeTask = stats.CountActiveAsync(query.TenantId, ct);
        var clicksTodayTask = stats.CountClicksTodayAsync(query.TenantId, ct);

        await Task.WhenAll(totalTask, activeTask, clicksTodayTask);

        return new TenantStatsResult(totalTask.Result, activeTask.Result, clicksTodayTask.Result);
    }
}
