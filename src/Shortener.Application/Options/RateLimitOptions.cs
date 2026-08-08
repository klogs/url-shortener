namespace Shortener.Application.Options;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public int PublicCreatePerMinute { get; init; } = 5;
    public int PublicCreatePerHour { get; init; } = 30;
}
