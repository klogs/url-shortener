using Shortener.Application.Billing;
using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Application.Domains.Commands.AddDomain;

public sealed class AddDomainHandler(IDomainRepository domains, ITenantRepository tenants, TimeProvider time)
{
    public async Task<AddDomainResult> HandleAsync(AddDomainCommand cmd, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmd.Host);

        var normalizedHost = TenantDomain.NormalizeHost(cmd.Host);

        var existing = await domains.GetByNormalizedHostAsync(normalizedHost, ct);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Host '{normalizedHost}' is already registered.");
        }

        var existingDomains = await domains.ListByTenantAsync(cmd.TenantId, ct);
        var isFirst = existingDomains.Count == 0;

        if (!isFirst)
        {
            var tenant = await tenants.GetByIdAsync(cmd.TenantId, ct)
                ?? throw new InvalidOperationException($"Tenant {cmd.TenantId} not found.");
            var limits = PlanLimitsProvider.For(tenant.Plan);
            if (limits.MaxCustomDomains != -1)
            {
                var customCount = await domains.CountCustomByTenantAsync(cmd.TenantId, ct);
                if (customCount >= limits.MaxCustomDomains)
                {
                    throw new InvalidOperationException(
                        $"Custom domain quota reached for your plan ({tenant.Plan}). Maximum {limits.MaxCustomDomains} custom domains allowed.");
                }
            }
        }
        var domain = TenantDomain.Create(cmd.TenantId, cmd.Host, isDefault: isFirst, time.GetUtcNow());

        await domains.InsertAsync(domain, ct);

        return new AddDomainResult(
            domain.Id,
            domain.Host,
            domain.NormalizedHost,
            domain.Status.ToString(),
            domain.IsDefault,
            domain.VerificationToken!);
    }
}
