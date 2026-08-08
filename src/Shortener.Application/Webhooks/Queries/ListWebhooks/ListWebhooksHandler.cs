using Shortener.Application.Interfaces;

namespace Shortener.Application.Webhooks.Queries.ListWebhooks;

public sealed class ListWebhooksHandler(IWebhookRepository webhooks)
{
    public async Task<IReadOnlyList<WebhookDto>> HandleAsync(ListWebhooksQuery query, CancellationToken ct = default)
    {
        var list = await webhooks.ListByTenantAsync(query.TenantId, ct);
        return list.Select(w => new WebhookDto(
            w.Id,
            w.Url,
            w.Events.Split(',', StringSplitOptions.RemoveEmptyEntries),
            w.IsActive,
            w.CreatedAtUtc)).ToList();
    }
}
