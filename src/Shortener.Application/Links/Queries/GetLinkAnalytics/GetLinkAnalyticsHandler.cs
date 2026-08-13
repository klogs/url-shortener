using Shortener.Application.Interfaces;

namespace Shortener.Application.Links.Queries.GetLinkAnalytics;

public sealed class GetLinkAnalyticsHandler(IClickEventRepository clickEvents, TimeProvider time)
{
    public async Task<LinkAnalyticsResult> HandleAsync(GetLinkAnalyticsQuery query, CancellationToken ct = default)
    {
        var days = Math.Clamp(query.Days, 1, 90);
        var to = time.GetUtcNow();
        var from = to.AddDays(-days);

        var dailyTask = clickEvents.GetDailyCountsAsync(query.LinkId, query.TenantId, from, to, ct);
        var countryTask = clickEvents.GetCountryBreakdownAsync(query.LinkId, query.TenantId, from, to, ct);
        var browserTask = clickEvents.GetBrowserBreakdownAsync(query.LinkId, query.TenantId, from, to, ct);
        await Task.WhenAll(dailyTask, countryTask, browserTask);
        var counts = dailyTask.Result;
        var countries = countryTask.Result;
        var browsers = browserTask.Result;

        var total = counts.Sum(c => c.Count);
        var series = counts.Select(c => new DailyPoint(c.Date, c.Count)).ToList();
        var countryBreakdown = countries.Select(c => new BreakdownItem(c.Country, c.Count)).ToList();
        var browserBreakdown = browsers
            .Select(b => new BreakdownItem(ParseBrowser(b.Browser), b.Count))
            .GroupBy(b => b.Label)
            .Select(g => new BreakdownItem(g.Key, g.Sum(x => x.Count)))
            .OrderByDescending(b => b.Count)
            .ToList();

        return new LinkAnalyticsResult(total, days, series, countryBreakdown, browserBreakdown);
    }

    /// <summary>
    /// Order matters: every Chromium-based browser also carries "Chrome/" in its
    /// user agent, so the derivatives (Edge, Opera, Samsung) must be matched
    /// before the plain Chrome check or they all collapse into "Chrome".
    /// </summary>
    private static string ParseBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) { return "Unknown"; }

        // Non-browser clients first — their agents are unambiguous.
        if (userAgent.Contains("curl/")) { return "curl"; }
        if (userAgent.Contains("python", StringComparison.OrdinalIgnoreCase)) { return "Python"; }
        if (userAgent.Contains("Go-http-client")) { return "Go"; }

        // Chromium derivatives, most specific first.
        if (userAgent.Contains("Edg/") || userAgent.Contains("EdgA/") || userAgent.Contains("EdgiOS/")) { return "Edge"; }
        if (userAgent.Contains("OPR/") || userAgent.Contains("Opera")) { return "Opera"; }
        if (userAgent.Contains("SamsungBrowser/")) { return "Samsung"; }
        // Brave ships a Chrome-identical agent by default to resist fingerprinting,
        // so this only catches the builds that opt into announcing themselves.
        if (userAgent.Contains("Brave/")) { return "Brave"; }
        if (userAgent.Contains("Chrome/") || userAgent.Contains("CriOS/")) { return "Chrome"; }

        if (userAgent.Contains("Firefox/") || userAgent.Contains("FxiOS/")) { return "Firefox"; }
        if (userAgent.Contains("Safari/")) { return "Safari"; }
        if (userAgent.Contains("MSIE") || userAgent.Contains("Trident")) { return "IE"; }

        return "Other";
    }
}

public sealed record DailyPoint(DateOnly Date, long Count);
public sealed record BreakdownItem(string Label, long Count);

public sealed record LinkAnalyticsResult(
    long Total,
    int Days,
    IReadOnlyList<DailyPoint> Series,
    IReadOnlyList<BreakdownItem> Countries,
    IReadOnlyList<BreakdownItem> Browsers);
