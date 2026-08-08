using Shortener.Application.Interfaces;
using Shortener.Domain.Enums;

namespace Shortener.Application.Links.Queries.ResolveRedirect;

public sealed class ResolveRedirectHandler(
    IShortLinkRepository links,
    ILinkVariantRepository variants,
    IGeoRouteRepository geoRoutes,
    IGeoResolver geoResolver,
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
            if (resolution.Outcome == RedirectOutcome.Redirect)
            {
                resolution = await ApplyGeoRoutingAsync(resolution, cached, query, ct);
                if (resolution.Outcome == RedirectOutcome.Redirect && cached.IsAbTest)
                {
                    var abDest = await ResolveAbVariantAsync(
                        query.NormalizedHost, query.ShortCode, cached.LinkId, resolution.TenantId, ct);
                    if (!string.IsNullOrEmpty(abDest))
                    {
                        return resolution with { DestinationUrl = abDest };
                    }
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
            link.IsAbTest,
            link.HasGeoRoutes);

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

        if (link.HasGeoRoutes && !string.IsNullOrEmpty(query.ClientIp))
        {
            var geoUrl = await ResolveGeoRouteAsync(query.NormalizedHost, query.ShortCode, link.Id, link.TenantId, query.ClientIp, ct);
            if (!string.IsNullOrEmpty(geoUrl))
            {
                destinationUrl = geoUrl;
            }
        }

        if (link.IsAbTest && destinationUrl == link.DestinationUrl)
        {
            var abUrl = await ResolveAbVariantAsync(
                query.NormalizedHost, query.ShortCode, link.Id, link.TenantId, ct);
            if (!string.IsNullOrEmpty(abUrl))
            {
                destinationUrl = abUrl;
            }
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

    private async Task<RedirectResolution> ApplyGeoRoutingAsync(
        RedirectResolution resolution, CachedRedirect cached, ResolveRedirectQuery query, CancellationToken ct)
    {
        if (!cached.HasGeoRoutes || string.IsNullOrEmpty(query.ClientIp))
        {
            return resolution;
        }

        var geoUrl = await ResolveGeoRouteAsync(
            query.NormalizedHost, query.ShortCode, cached.LinkId, resolution.TenantId, query.ClientIp, ct);

        return string.IsNullOrEmpty(geoUrl) ? resolution : resolution with { DestinationUrl = geoUrl };
    }

    private async Task<string?> ResolveGeoRouteAsync(
        string normalizedHost, string shortCode, Guid linkId, Guid? tenantId, string clientIp, CancellationToken ct)
    {
        var cachedRoutes = await cache.GetGeoRoutesAsync(normalizedHost, shortCode, ct);
        if (cachedRoutes is null)
        {
            var dbRoutes = await geoRoutes.ListByLinkAsync(linkId, tenantId ?? Guid.Empty, ct);
            cachedRoutes = dbRoutes.Select(r => new CachedGeoRoute(r.CountryCode, r.DestinationUrl)).ToList();
            await cache.SetGeoRoutesAsync(normalizedHost, shortCode, cachedRoutes, ct);
        }

        if (cachedRoutes.Count == 0)
        {
            return null;
        }

        var country = await geoResolver.GetCountryAsync(clientIp, ct);
        if (string.IsNullOrEmpty(country))
        {
            return null;
        }

        var match = cachedRoutes.FirstOrDefault(r =>
            string.Equals(r.CountryCode, country, StringComparison.OrdinalIgnoreCase));

        return match?.DestinationUrl;
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
