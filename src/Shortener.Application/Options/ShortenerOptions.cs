namespace Shortener.Application.Options;

public sealed class ShortenerOptions
{
    public const string SectionName = "App";

    public string DefaultDomain { get; init; } = string.Empty;
    public int AnonymousExpirationDays { get; init; } = 7;
    public int ShortCodeLength { get; init; } = 7;
    public string MultitenancyMode { get; init; } = "SingleTenant";

    // Optional: pin the default tenant to a known ID (e.g. the IDP's tenant claim value).
    // When set, SingleTenantSeeder uses this ID instead of generating a new one.
    public Guid? DefaultTenantId { get; init; }
}
