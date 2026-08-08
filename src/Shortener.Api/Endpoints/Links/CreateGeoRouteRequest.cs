namespace Shortener.Api.Endpoints.Links;

public sealed record CreateGeoRouteRequest(string CountryCode, string DestinationUrl);
