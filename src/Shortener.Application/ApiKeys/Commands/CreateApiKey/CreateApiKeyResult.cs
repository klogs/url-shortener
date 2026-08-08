namespace Shortener.Application.ApiKeys.Commands.CreateApiKey;

public sealed record CreateApiKeyResult(
    Guid Id,
    string Name,
    string KeyPrefix,
    /// <summary>Full raw key — returned once on creation, never stored or retrievable again.</summary>
    string RawKey,
    IReadOnlyList<string> Scopes,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset CreatedAtUtc);
