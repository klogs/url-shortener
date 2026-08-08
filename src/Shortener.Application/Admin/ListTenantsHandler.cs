using Shortener.Application.Interfaces;
using Shortener.Domain.Enums;

namespace Shortener.Application.Admin;

public sealed record ListTenantsQuery(int PageSize = 25, Guid? AfterId = null);

public sealed record TenantSummary(Guid Id, string Name, TenantPlan Plan, bool IsActive, DateTimeOffset CreatedAtUtc);

public sealed record ListTenantsResult(IReadOnlyList<TenantSummary> Items, bool HasMore);

public sealed class ListTenantsHandler(ITenantRepository tenants)
{
    public async Task<ListTenantsResult> HandleAsync(ListTenantsQuery query, CancellationToken ct = default)
    {
        var (items, hasMore) = await tenants.ListAllAsync(query.PageSize, query.AfterId, ct);
        var summaries = items
            .Select(t => new TenantSummary(t.Id, t.Name, t.Plan, t.IsActive, t.CreatedAtUtc))
            .ToList();
        return new ListTenantsResult(summaries, hasMore);
    }
}
