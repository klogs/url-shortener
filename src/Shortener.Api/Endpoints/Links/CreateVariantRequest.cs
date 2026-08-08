namespace Shortener.Api.Endpoints.Links;

public sealed record CreateVariantRequest(string Label, string DestinationUrl, int Weight = 1);
