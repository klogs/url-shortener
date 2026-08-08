namespace Shortener.Application.Interfaces;

public interface IWebhookDispatcher
{
    Task DispatchAsync(Guid tenantId, string eventType, string payload, CancellationToken ct = default);
}
