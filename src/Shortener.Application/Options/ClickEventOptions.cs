namespace Shortener.Application.Options;

public sealed class ClickEventOptions
{
    public const string SectionName = "Analytics";

    public int ChannelCapacity { get; init; } = 10_000;
    public string Exchange { get; init; } = "analytics";
    public string Queue { get; init; } = "analytics.click-events";
    public string RoutingKey { get; init; } = "click.event";
    public int RawRetentionDays { get; init; } = 90;
}
