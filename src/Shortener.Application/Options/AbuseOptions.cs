namespace Shortener.Application.Options;

public sealed class AbuseOptions
{
    public const string SectionName = "Abuse";

    public int AutoBlockThreshold { get; init; } = 10;
    // Semicolon-separated blocked destination hosts, e.g. "malware.com;phishing.net"
    public string BlockedHosts { get; init; } = string.Empty;
}
