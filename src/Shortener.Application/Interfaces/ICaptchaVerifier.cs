namespace Shortener.Application.Interfaces;

public interface ICaptchaVerifier
{
    Task<bool> VerifyAsync(string token, string? clientIp, CancellationToken ct = default);
}
