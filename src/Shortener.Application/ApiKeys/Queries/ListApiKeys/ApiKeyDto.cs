namespace Shortener.Application.ApiKeys.Queries.ListApiKeys;

public sealed record ApiKeyDto(
    Guid Id,
    string Name,
    string KeyPrefix,
    IReadOnlyList<string> Scopes,
    bool IsRevoked,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? LastUsedAtUtc,
    DateTimeOffset CreatedAtUtc);
