using Shortener.Application.Interfaces;

namespace Shortener.Application.GeoRoutes.Commands.DeleteGeoRoute;

public sealed class DeleteGeoRouteHandler(IShortLinkRepository links, IGeoRouteRepository routes)
{
    public async Task HandleAsync(DeleteGeoRouteCommand cmd, CancellationToken ct = default)
    {
        var route = await routes.GetByIdAsync(cmd.RouteId, cmd.TenantId, ct)
            ?? throw new InvalidOperationException("Geo route not found.");

        await routes.DeleteAsync(route.Id, ct);

        var remaining = await routes.CountByLinkAsync(route.LinkId, ct);
        if (remaining == 0)
        {
            var link = await links.GetByIdAsync(route.LinkId, cmd.TenantId, ct);
            if (link is not null && link.HasGeoRoutes)
            {
                link.DisableGeoRouting();
                await links.UpdateAsync(link, ct);
            }
        }
    }
}
