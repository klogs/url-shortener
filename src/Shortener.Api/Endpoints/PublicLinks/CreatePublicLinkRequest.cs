namespace Shortener.Api.Endpoints.PublicLinks;

public sealed record CreatePublicLinkRequest(
    string DestinationUrl,
    string? CaptchaToken = null);
