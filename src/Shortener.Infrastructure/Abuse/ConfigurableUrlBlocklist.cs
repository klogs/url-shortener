using Microsoft.Extensions.Options;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;

namespace Shortener.Infrastructure.Abuse;

internal sealed class ConfigurableUrlBlocklist : IUrlBlocklist
{
    private readonly HashSet<string> _blockedHosts;

    public ConfigurableUrlBlocklist(IOptions<AbuseOptions> opts)
    {
        _blockedHosts = opts.Value.BlockedHosts
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(h => h.ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public bool IsBlocked(string destinationUrl)
    {
        if (!Uri.TryCreate(destinationUrl, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var host = uri.Host.ToLowerInvariant();

        // Exact match or subdomain match: "evil.com" blocks "sub.evil.com"
        return _blockedHosts.Any(blocked =>
            host == blocked || host.EndsWith("." + blocked, StringComparison.OrdinalIgnoreCase));
    }
}
