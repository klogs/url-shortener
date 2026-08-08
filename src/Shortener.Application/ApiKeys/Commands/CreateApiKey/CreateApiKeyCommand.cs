namespace Shortener.Application.ApiKeys.Commands.CreateApiKey;

public sealed record CreateApiKeyCommand(
    Guid TenantId,
    string Name,
    IReadOnlyList<string> Scopes,
    DateTimeOffset? ExpiresAt);
