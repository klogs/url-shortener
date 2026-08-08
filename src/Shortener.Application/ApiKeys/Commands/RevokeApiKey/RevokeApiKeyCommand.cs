namespace Shortener.Application.ApiKeys.Commands.RevokeApiKey;

public sealed record RevokeApiKeyCommand(Guid ApiKeyId, Guid TenantId);
