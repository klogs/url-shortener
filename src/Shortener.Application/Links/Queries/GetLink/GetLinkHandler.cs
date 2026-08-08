using Shortener.Application.Interfaces;

namespace Shortener.Application.Links.Queries.GetLink;

public sealed class GetLinkHandler(IShortLinkRepository links)
{
    public async Task<LinkDetail?> HandleAsync(GetLinkQuery query, CancellationToken ct = default)
    {
        var link = await links.GetByIdAsync(query.Id, query.TenantId, ct);
        if (link is null)
        {
            return null;
        }

        return new LinkDetail(
            link.Id, link.DomainId, link.ShortCode, link.DestinationUrl,
            link.Title, link.Description, link.Status.ToString(), link.RedirectType,
            link.IsAnonymous, link.CreatedAtUtc, link.CreatedBy, link.UpdatedAtUtc,
            link.ExpiresAt, link.LastAccessedAt, link.ClickCountSnapshot);
    }
}
