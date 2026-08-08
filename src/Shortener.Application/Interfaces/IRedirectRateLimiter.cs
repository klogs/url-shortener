namespace Shortener.Application.Interfaces;

public interface IRedirectRateLimiter
{
    /// <summary>Returns true if the request is within the allowed rate; false if it should be rejected with 429.</summary>
    Task<bool> IsAllowedAsync(string ip, CancellationToken ct = default);
}
