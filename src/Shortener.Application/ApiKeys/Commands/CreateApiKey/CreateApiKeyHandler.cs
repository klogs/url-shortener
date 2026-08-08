using System.Security.Cryptography;
using System.Text;
using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Application.ApiKeys.Commands.CreateApiKey;

public sealed class CreateApiKeyHandler(IApiKeyRepository apiKeys, TimeProvider time)
{
    private const int KeyByteLength = 32; // 256-bit → 43-char base64url
    private const int PrefixLength = 8;

    public async Task<CreateApiKeyResult> HandleAsync(CreateApiKeyCommand cmd, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmd.Name);

        if (cmd.Scopes.Count == 0)
        {
            throw new ArgumentException("At least one scope is required.");
        }

        var rawBytes = RandomNumberGenerator.GetBytes(KeyByteLength);
        var rawKey = Convert.ToBase64String(rawBytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('='); // base64url

        var prefix = rawKey[..PrefixLength];
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

        var apiKey = ApiKey.Create(
            cmd.TenantId,
            cmd.Name,
            prefix,
            hash,
            string.Join(' ', cmd.Scopes),
            cmd.ExpiresAt,
            time.GetUtcNow());

        await apiKeys.InsertAsync(apiKey, ct);

        return new CreateApiKeyResult(
            apiKey.Id,
            apiKey.Name,
            apiKey.KeyPrefix,
            rawKey,
            cmd.Scopes,
            apiKey.ExpiresAt,
            apiKey.CreatedAtUtc);
    }
}
