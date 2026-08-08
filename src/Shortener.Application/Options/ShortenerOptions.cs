namespace Shortener.Application.Options;

public sealed class ShortenerOptions
{
    public const string SectionName = "App";

    public string DefaultDomain { get; init; } = string.Empty;
    public int AnonymousExpirationDays { get; init; } = 7;
    public int ShortCodeLength { get; init; } = 7;
    public string MultitenancyMode { get; init; } = "SingleTenant";
}
