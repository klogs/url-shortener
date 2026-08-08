namespace Shortener.Application.GeoRoutes.Commands.DeleteGeoRoute;

public sealed record DeleteGeoRouteCommand(Guid RouteId, Guid TenantId);
