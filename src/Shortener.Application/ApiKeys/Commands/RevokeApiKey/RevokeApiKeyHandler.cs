using Shortener.Application.Interfaces;

namespace Shortener.Application.ApiKeys.Commands.RevokeApiKey;

public sealed class RevokeApiKeyHandler(IApiKeyRepository apiKeys)
{
    public async Task HandleAsync(RevokeApiKeyCommand cmd, CancellationToken ct = default)
    {
        var key = await apiKeys.GetByIdAsync(cmd.ApiKeyId, cmd.TenantId, ct)
            ?? throw new InvalidOperationException("API key not found.");

        key.Revoke();
        await apiKeys.UpdateAsync(key, ct);
    }
}
