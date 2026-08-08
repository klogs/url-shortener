namespace Shortener.Application.Links.Commands.BulkCreateLinks;

public sealed record BulkLinkItem(string DestinationUrl, string? Alias, DateTimeOffset? ExpiresAt);

public sealed record BulkCreateLinksCommand(
    Guid TenantId,
    Guid DomainId,
    Guid CreatedBy,
    IReadOnlyList<BulkLinkItem> Links);
