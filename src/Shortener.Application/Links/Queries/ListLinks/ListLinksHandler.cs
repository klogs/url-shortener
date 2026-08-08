using Shortener.Application.Interfaces;

namespace Shortener.Application.Links.Queries.ListLinks;

public sealed class ListLinksHandler(IShortLinkRepository links)
{
    public async Task<ListLinksResult> HandleAsync(ListLinksQuery query, CancellationToken ct = default)
    {
        var size = Math.Clamp(query.PageSize, 1, 100);
        var (items, hasMore) = await links.ListAsync(query.TenantId, size, query.AfterId, ct);

        var summaries = items.Select(l => new LinkSummary(
            l.Id, l.ShortCode, l.DestinationUrl, l.Title,
            l.Status.ToString(), l.CreatedAtUtc, l.ExpiresAt, l.ClickCountSnapshot))
            .ToList();

        var nextCursor = hasMore ? summaries[^1].Id : (Guid?)null;
        return new ListLinksResult(summaries, nextCursor);
    }
}
