namespace Shortener.Application.GeoRoutes.Commands.CreateGeoRoute;

public sealed record CreateGeoRouteCommand(
    Guid LinkId,
    Guid TenantId,
    string CountryCode,
    string DestinationUrl);
