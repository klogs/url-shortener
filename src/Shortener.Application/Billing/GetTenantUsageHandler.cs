using Shortener.Application.Interfaces;
using Shortener.Domain.Enums;
using Shortener.Domain.ValueObjects;

namespace Shortener.Application.Billing;

public sealed record GetTenantUsageQuery(Guid TenantId);

public sealed record TenantUsageResult(
    int LinkCount,
    int CustomDomainCount,
    TenantPlan Plan,
    PlanLimits Limits);

public sealed class GetTenantUsageHandler(
    ITenantRepository tenants,
    IShortLinkRepository links,
    IDomainRepository domains)
{
    public async Task<TenantUsageResult> HandleAsync(GetTenantUsageQuery query, CancellationToken ct = default)
    {
        var tenant = await tenants.GetByIdAsync(query.TenantId, ct)
            ?? throw new InvalidOperationException($"Tenant {query.TenantId} not found.");

        var linkCount   = await links.CountActiveByTenantAsync(query.TenantId, ct);
        var domainCount = await domains.CountCustomByTenantAsync(query.TenantId, ct);
        var limits      = PlanLimitsProvider.For(tenant.Plan);

        return new TenantUsageResult(linkCount, domainCount, tenant.Plan, limits);
    }
}
