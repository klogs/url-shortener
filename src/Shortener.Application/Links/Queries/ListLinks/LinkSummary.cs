namespace Shortener.Application.Links.Queries.ListLinks;

public sealed record LinkSummary(
    Guid Id,
    string ShortCode,
    string DestinationUrl,
    string? Title,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ExpiresAt,
    long ClickCountSnapshot);

public sealed record ListLinksResult(IReadOnlyList<LinkSummary> Items, Guid? NextCursor);
