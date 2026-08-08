using Shortener.Domain.Entities;

namespace Shortener.Application.Interfaces;

public interface IApiKeyAuthenticator
{
    /// <summary>
    /// Resolves the ApiKey record for a raw key value.
    /// Returns null if the key is invalid, revoked, or expired.
    /// </summary>
    Task<ApiKey?> AuthenticateAsync(string rawKey, CancellationToken ct = default);
}
