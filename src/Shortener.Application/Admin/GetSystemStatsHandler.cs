using Shortener.Application.Interfaces;

namespace Shortener.Application.Admin;

public sealed record GetSystemStatsQuery;

public sealed record SystemStatsResult(int TotalTenants, int TotalLinks);

public sealed class GetSystemStatsHandler(ITenantRepository tenants, IShortLinkRepository links)
{
    public async Task<SystemStatsResult> HandleAsync(GetSystemStatsQuery query, CancellationToken ct = default)
    {
        var tenantCount = await tenants.CountAllAsync(ct);
        var linkCount = await links.CountAllAsync(ct);
        return new SystemStatsResult(tenantCount, linkCount);
    }
}
