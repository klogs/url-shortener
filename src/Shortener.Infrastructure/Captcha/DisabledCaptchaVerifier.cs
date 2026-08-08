using Shortener.Application.Interfaces;

namespace Shortener.Infrastructure.Captcha;

internal sealed class DisabledCaptchaVerifier : ICaptchaVerifier
{
    public Task<bool> VerifyAsync(string token, string? clientIp, CancellationToken ct)
        => Task.FromResult(true);
}
