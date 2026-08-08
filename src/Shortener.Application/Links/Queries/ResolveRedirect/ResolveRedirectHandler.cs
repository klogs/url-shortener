using Shortener.Application.Interfaces;
using Shortener.Domain.Enums;

namespace Shortener.Application.Links.Queries.ResolveRedirect;

public sealed class ResolveRedirectHandler(
    IShortLinkRepository links,
    IRedirectCache cache,
    TimeProvider time)
{
    public async Task<RedirectResolution> HandleAsync(ResolveRedirectQuery query, CancellationToken ct = default)
    {
        var now = time.GetUtcNow();

        // 1. Redis cache lookup
        var cached = await cache.GetAsync(query.NormalizedHost, query.ShortCode, ct);
        if (cached is not null)
        {
            return ResolveCached(cached, now);
        }

        // 2. PostgreSQL fallback
        var link = await links.GetByHostAndCodeAsync(query.NormalizedHost, query.ShortCode, ct);
        if (link is null)
        {
            await cache.SetNegativeAsync(query.NormalizedHost, query.ShortCode, ct);
            return new RedirectResolution(RedirectOutcome.NotFound);
        }

        // 3. Populate cache for subsequent requests
        var entry = new CachedRedirect(
            link.Id,
            link.DestinationUrl,
            link.Status.ToString(),
            link.ExpiresAt,
            (int)link.RedirectType);

        await cache.SetAsync(query.NormalizedHost, query.ShortCode, entry, link.ExpiresAt, ct);

        // 4. Runtime expiry/status check — correctness never depends on sweep worker
        if (link.Status == LinkStatus.Deleted || link.Status == LinkStatus.Blocked)
        {
            return new RedirectResolution(RedirectOutcome.NotFound);
        }

        if (link.IsExpired(now) || link.Status == LinkStatus.Expired)
        {
            return new RedirectResolution(RedirectOutcome.Expired);
        }

        if (link.Status == LinkStatus.Disabled)
        {
            return new RedirectResolution(RedirectOutcome.Disabled);
        }

        return new RedirectResolution(RedirectOutcome.Redirect, link.DestinationUrl, (int)link.RedirectType,
            LinkId: link.Id, TenantId: link.TenantId);
    }

    private static RedirectResolution ResolveCached(CachedRedirect cached, DateTimeOffset now)
    {
        // Negative cache sentinel: empty destination means "not found"
        if (cached.DestinationUrl == string.Empty)
        {
            return new RedirectResolution(RedirectOutcome.NotFound);
        }

        if (cached.ExpiresAt.HasValue && cached.ExpiresAt.Value <= now)
        {
            return new RedirectResolution(RedirectOutcome.Expired);
        }

        if (cached.Status is "Disabled")
        {
            return new RedirectResolution(RedirectOutcome.Disabled);
        }

        if (cached.Status is "Deleted" or "Blocked")
        {
            return new RedirectResolution(RedirectOutcome.NotFound);
        }

        return new RedirectResolution(RedirectOutcome.Redirect, cached.DestinationUrl, cached.RedirectStatusCode,
            LinkId: cached.LinkId, TenantId: null); // TenantId not in cache — analytics can be added via LinkId join
    }
}
