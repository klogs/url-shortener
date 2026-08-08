using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Application.GeoRoutes.Commands.CreateGeoRoute;

public sealed class CreateGeoRouteHandler(
    IShortLinkRepository links,
    IGeoRouteRepository routes,
    TimeProvider time)
{
    public async Task<CreateGeoRouteResult> HandleAsync(CreateGeoRouteCommand cmd, CancellationToken ct = default)
    {
        // Verify the link exists and belongs to the tenant
        _ = await links.GetByIdAsync(cmd.LinkId, cmd.TenantId, ct)
            ?? throw new InvalidOperationException("Link not found.");

        var route = GeoRoute.Create(cmd.LinkId, cmd.TenantId, cmd.CountryCode, cmd.DestinationUrl, time.GetUtcNow());
        await routes.InsertAsync(route, ct);

        return new CreateGeoRouteResult(route.Id, route.LinkId, route.CountryCode, route.DestinationUrl, route.CreatedAtUtc);
    }
}
