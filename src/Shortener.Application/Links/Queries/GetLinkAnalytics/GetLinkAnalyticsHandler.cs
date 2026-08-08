using Shortener.Application.Interfaces;

namespace Shortener.Application.Links.Queries.GetLinkAnalytics;

public sealed class GetLinkAnalyticsHandler(IClickEventRepository clickEvents, TimeProvider time)
{
    public async Task<LinkAnalyticsResult> HandleAsync(GetLinkAnalyticsQuery query, CancellationToken ct = default)
    {
        var days = Math.Clamp(query.Days, 1, 90);
        var to = time.GetUtcNow();
        var from = to.AddDays(-days);

        var counts = await clickEvents.GetDailyCountsAsync(query.LinkId, query.TenantId, from, to, ct);

        var total = counts.Sum(c => c.Count);
        var series = counts
            .Select(c => new DailyPoint(c.Date, c.Count))
            .ToList();

        return new LinkAnalyticsResult(total, days, series);
    }
}

public sealed record DailyPoint(DateOnly Date, long Count);

public sealed record LinkAnalyticsResult(long Total, int Days, IReadOnlyList<DailyPoint> Series);
