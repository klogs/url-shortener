namespace Shortener.Application.GeoRoutes.Queries;

public sealed record GeoRouteDto(
    Guid Id,
    Guid LinkId,
    string CountryCode,
    string DestinationUrl,
    DateTimeOffset CreatedAtUtc);
