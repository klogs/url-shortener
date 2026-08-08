using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.Webhooks;

internal sealed class WebhookDispatcher(IWebhookRepository webhooks, TimeProvider time) : IWebhookDispatcher
{
    public async Task DispatchAsync(Guid tenantId, string eventType, string payload, CancellationToken ct)
    {
        var tenantWebhooks = await webhooks.ListByTenantAsync(tenantId, ct);
        var now = time.GetUtcNow();

        foreach (var webhook in tenantWebhooks)
        {
            if (!webhook.IsActive || !webhook.SubscribesTo(eventType))
            {
                continue;
            }

            var delivery = WebhookDelivery.Create(webhook.Id, tenantId, eventType, payload, now);
            await webhooks.InsertDeliveryAsync(delivery, ct);
        }
    }
}
