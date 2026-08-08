namespace Shortener.Application.Options;

public sealed class CaptchaOptions
{
    public const string SectionName = "Captcha";

    public string Provider { get; init; } = "Disabled";
    public string SiteKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
}
