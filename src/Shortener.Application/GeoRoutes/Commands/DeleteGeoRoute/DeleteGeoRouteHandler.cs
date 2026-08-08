using Shortener.Application.Interfaces;

namespace Shortener.Application.GeoRoutes.Commands.DeleteGeoRoute;

public sealed class DeleteGeoRouteHandler(IGeoRouteRepository routes)
{
    public async Task HandleAsync(DeleteGeoRouteCommand cmd, CancellationToken ct = default)
    {
        _ = await routes.GetByIdAsync(cmd.RouteId, cmd.TenantId, ct)
            ?? throw new InvalidOperationException("Geo route not found.");

        await routes.DeleteAsync(cmd.RouteId, ct);
    }
}
