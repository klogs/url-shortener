namespace Shortener.Api.Endpoints.ApiKeys;

public sealed record BulkLinkItemRequest(string DestinationUrl, string? Alias, DateTimeOffset? ExpiresAt);

public sealed record BulkCreateRequest(Guid DomainId, IReadOnlyList<BulkLinkItemRequest> Links);
