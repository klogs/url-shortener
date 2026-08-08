namespace Shortener.Api.Endpoints.ApiKeys;

public sealed record CreateApiKeyRequest(
    string Name,
    IReadOnlyList<string> Scopes,
    DateTimeOffset? ExpiresAt);
