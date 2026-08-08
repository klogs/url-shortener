using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Application.Webhooks.Queries;

public sealed record GetWebhookDeliveriesQuery(Guid WebhookId, Guid TenantId, int PageSize = 20);

public sealed class GetWebhookDeliveriesHandler(IWebhookRepository webhooks)
{
    public Task<IReadOnlyList<WebhookDelivery>> HandleAsync(
        GetWebhookDeliveriesQuery query, CancellationToken ct = default)
        => webhooks.ListDeliveriesByWebhookAsync(query.WebhookId, query.TenantId, query.PageSize, ct);
}
