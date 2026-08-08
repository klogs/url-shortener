using Shortener.Domain.Enums;

namespace Shortener.Application.Links.Queries.GetLink;

public sealed record LinkDetail(
    Guid Id,
    Guid DomainId,
    string ShortCode,
    string DestinationUrl,
    string? Title,
    string? Description,
    string Status,
    RedirectType RedirectType,
    bool IsAnonymous,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedBy,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? LastAccessedAt,
    long ClickCountSnapshot);
