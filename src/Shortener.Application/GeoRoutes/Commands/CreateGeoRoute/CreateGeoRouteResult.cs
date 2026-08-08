namespace Shortener.Application.GeoRoutes.Commands.CreateGeoRoute;

public sealed record CreateGeoRouteResult(
    Guid Id,
    Guid LinkId,
    string CountryCode,
    string DestinationUrl,
    DateTimeOffset CreatedAtUtc);
