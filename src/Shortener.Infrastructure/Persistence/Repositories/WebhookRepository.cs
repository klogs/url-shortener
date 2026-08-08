using Microsoft.EntityFrameworkCore;
using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;
using Shortener.Domain.Enums;

namespace Shortener.Infrastructure.Persistence.Repositories;

internal sealed class WebhookRepository(ShortenerDbContext db) : IWebhookRepository
{
    public Task<Webhook?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
        => db.Webhooks.FirstOrDefaultAsync(w => w.Id == id && w.TenantId == tenantId, ct);

    public async Task<IReadOnlyList<Webhook>> ListByTenantAsync(Guid tenantId, CancellationToken ct)
        => await db.Webhooks
            .Where(w => w.TenantId == tenantId)
            .OrderBy(w => w.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task InsertAsync(Webhook webhook, CancellationToken ct)
    {
        db.Webhooks.Add(webhook);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Webhook webhook, CancellationToken ct)
    {
        db.Webhooks.Update(webhook);
        await db.SaveChangesAsync(ct);
    }

    public async Task InsertDeliveryAsync(WebhookDelivery delivery, CancellationToken ct)
    {
        db.WebhookDeliveries.Add(delivery);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<WebhookDelivery>> ListPendingDeliveriesAsync(DateTimeOffset now, int batchSize, CancellationToken ct)
        => await db.WebhookDeliveries
            .Where(d => d.Status == DeliveryStatus.Pending && d.NextAttemptAt <= now)
            .OrderBy(d => d.NextAttemptAt)
            .Take(batchSize)
            .ToListAsync(ct);

    public async Task UpdateDeliveryAsync(WebhookDelivery delivery, CancellationToken ct)
    {
        db.WebhookDeliveries.Update(delivery);
        await db.SaveChangesAsync(ct);
    }
}
