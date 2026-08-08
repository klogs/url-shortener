using Shortener.Domain.Enums;

namespace Shortener.Domain.Entities;

public sealed class WebhookDelivery
{
    private WebhookDelivery() { }

    public Guid Id { get; private set; }
    public Guid WebhookId { get; private set; }
    public Guid TenantId { get; private set; }
    public string EventType { get; private set; } = default!;
    // JSON payload to deliver.
    public string Payload { get; private set; } = default!;
    public DeliveryStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset? NextAttemptAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public string? LastResponseBody { get; private set; }
    public int? LastHttpStatus { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static WebhookDelivery Create(
        Guid webhookId,
        Guid tenantId,
        string eventType,
        string payload,
        DateTimeOffset now)
    {
        return new WebhookDelivery
        {
            Id = Guid.NewGuid(),
            WebhookId = webhookId,
            TenantId = tenantId,
            EventType = eventType,
            Payload = payload,
            Status = DeliveryStatus.Pending,
            AttemptCount = 0,
            NextAttemptAt = now,
            CreatedAtUtc = now,
        };
    }

    public void RecordSuccess(int httpStatus, string? responseBody, DateTimeOffset now)
    {
        Status = DeliveryStatus.Delivered;
        AttemptCount++;
        LastHttpStatus = httpStatus;
        LastResponseBody = responseBody;
        DeliveredAt = now;
        NextAttemptAt = null;
    }

    public void RecordFailure(int? httpStatus, string? responseBody, DateTimeOffset now, int maxAttempts = 5)
    {
        AttemptCount++;
        LastHttpStatus = httpStatus;
        LastResponseBody = responseBody;

        if (AttemptCount >= maxAttempts)
        {
            Status = DeliveryStatus.Failed;
            NextAttemptAt = null;
        }
        else
        {
            // Exponential backoff: 1m, 5m, 15m, 1h, 4h
            var delayMinutes = AttemptCount switch
            {
                1 => 5,
                2 => 15,
                3 => 60,
                _ => 240,
            };
            NextAttemptAt = now.AddMinutes(delayMinutes);
        }
    }
}
