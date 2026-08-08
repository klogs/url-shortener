using Shortener.Application.Interfaces;

namespace Shortener.Infrastructure.Caching;

internal sealed class LinkCacheInvalidator(
    IDomainRepository domains,
    IRedirectCache redirectCache,
    IEdgeCacheInvalidator edgeCache) : ILinkCacheInvalidator
{
    public async Task InvalidateAsync(Guid domainId, Guid tenantId, string shortCode, CancellationToken ct = default)
    {
        var domain = await domains.GetByIdAsync(domainId, tenantId, ct);
        if (domain is null)
        {
            return;
        }

        await redirectCache.RemoveAsync(domain.NormalizedHost, shortCode, ct);
        await edgeCache.InvalidateAsync(domain.NormalizedHost, shortCode, ct);
    }
}
