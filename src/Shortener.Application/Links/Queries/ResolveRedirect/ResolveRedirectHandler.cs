using Shortener.Application.Interfaces;
using Shortener.Domain.Enums;

namespace Shortener.Application.Links.Queries.ResolveRedirect;

public sealed class ResolveRedirectHandler(
    IShortLinkRepository links,
    ILinkVariantRepository variants,
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
            var resolution = ResolveCached(cached, now);
            if (resolution.Outcome == RedirectOutcome.Redirect && cached.IsAbTest)
            {
                var destination = await ResolveAbVariantAsync(
                    query.NormalizedHost, query.ShortCode, cached.LinkId, resolution.TenantId, ct);
                if (!string.IsNullOrEmpty(destination))
                {
                    return resolution with { DestinationUrl = destination };
                }
            }

            return resolution;
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
            (int)link.RedirectType,
            link.IsAbTest);

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

        var destinationUrl = link.DestinationUrl;
        if (link.IsAbTest)
        {
            destinationUrl = await ResolveAbVariantAsync(
                query.NormalizedHost, query.ShortCode, link.Id, link.TenantId, ct);
        }

        return new RedirectResolution(RedirectOutcome.Redirect, destinationUrl, (int)link.RedirectType,
            LinkId: link.Id, TenantId: link.TenantId);
    }

    private async Task<string> ResolveAbVariantAsync(
        string normalizedHost, string shortCode, Guid linkId, Guid? tenantId, CancellationToken ct)
    {
        var cached = await cache.GetVariantsAsync(normalizedHost, shortCode, ct);
        if (cached is null || cached.Count == 0)
        {
            var dbVariants = await variants.ListByLinkAsync(linkId, tenantId ?? Guid.Empty, ct);
            if (dbVariants.Count == 0)
            {
                // No variants configured — fall through to default URL
                return string.Empty;
            }

            cached = dbVariants.Select(v => new CachedVariant(v.Weight, v.DestinationUrl)).ToList();
            await cache.SetVariantsAsync(normalizedHost, shortCode, cached, ct);
        }

        return PickWeightedVariant(cached);
    }

    private static string PickWeightedVariant(IReadOnlyList<CachedVariant> variantList)
    {
        var total = variantList.Sum(v => v.Weight);
        var r = Random.Shared.Next(total);
        foreach (var v in variantList)
        {
            r -= v.Weight;
            if (r < 0)
            {
                return v.DestinationUrl;
            }
        }

        return variantList[^1].DestinationUrl;
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
