namespace Shortener.Application.Interfaces;

public interface IDomainVerifier
{
    /// <summary>
    /// Fetches https://{host}/.well-known/shortener-verify and checks the response body
    /// contains <paramref name="expectedToken"/>. Returns true if verified.
    /// </summary>
    Task<bool> VerifyAsync(string host, string expectedToken, CancellationToken ct = default);
}
