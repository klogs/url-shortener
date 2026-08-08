using Shortener.Application.Interfaces;

namespace Shortener.Application.GeoRoutes.Queries;

public sealed class ListGeoRoutesHandler(IGeoRouteRepository routes)
{
    public async Task<IReadOnlyList<GeoRouteDto>> HandleAsync(ListGeoRoutesQuery query, CancellationToken ct = default)
    {
        var items = await routes.ListByLinkAsync(query.LinkId, query.TenantId, ct);
        return items
            .Select(r => new GeoRouteDto(r.Id, r.LinkId, r.CountryCode, r.DestinationUrl, r.CreatedAtUtc))
            .ToList();
    }
}
