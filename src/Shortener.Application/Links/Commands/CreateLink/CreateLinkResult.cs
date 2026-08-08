namespace Shortener.Application.Links.Commands.CreateLink;

public sealed record CreateLinkResult(
    Guid Id,
    string ShortCode,
    string ShortUrl,
    string DestinationUrl,
    string Status,
    DateTimeOffset? ExpiresAt);
