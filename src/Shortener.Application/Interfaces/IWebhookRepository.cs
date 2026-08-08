using Shortener.Domain.Entities;

namespace Shortener.Application.Interfaces;

public interface IWebhookRepository
{
    Task<Webhook?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Webhook>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task InsertAsync(Webhook webhook, CancellationToken ct = default);
    Task UpdateAsync(Webhook webhook, CancellationToken ct = default);

    Task InsertDeliveryAsync(WebhookDelivery delivery, CancellationToken ct = default);
    Task<IReadOnlyList<WebhookDelivery>> ListPendingDeliveriesAsync(DateTimeOffset now, int batchSize, CancellationToken ct = default);
    Task UpdateDeliveryAsync(WebhookDelivery delivery, CancellationToken ct = default);
}
