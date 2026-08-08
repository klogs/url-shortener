using Shortener.Domain.Enums;

namespace Shortener.Api.Endpoints.Links;

public sealed record UpdateLinkRequest(
    string DestinationUrl,
    string? Title = null,
    DateTimeOffset? ExpiresAt = null,
    RedirectType RedirectType = RedirectType.Found);
