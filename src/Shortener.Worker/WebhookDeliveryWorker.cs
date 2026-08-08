using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shortener.Application.Interfaces;

namespace Shortener.Worker;

internal sealed class WebhookDeliveryWorker(
    IServiceScopeFactory scopeFactory,
    TimeProvider time,
    ILogger<WebhookDeliveryWorker> logger)
    : BackgroundService
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(15) };
    private const int BatchSize = 20;
    private const int MaxAttempts = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in webhook delivery worker.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var webhookRepo = scope.ServiceProvider.GetRequiredService<IWebhookRepository>();

        var now = time.GetUtcNow();
        var pending = await webhookRepo.ListPendingDeliveriesAsync(now, BatchSize, ct);

        foreach (var delivery in pending)
        {
            var webhook = await webhookRepo.GetByIdAsync(delivery.WebhookId, delivery.TenantId, ct);
            if (webhook is null || !webhook.IsActive)
            {
                delivery.RecordFailure(null, "Webhook not found or inactive.", now, MaxAttempts);
                await webhookRepo.UpdateDeliveryAsync(delivery, ct);
                continue;
            }

            try
            {
                var content = new StringContent(delivery.Payload, Encoding.UTF8, "application/json");
                var signature = ComputeSignature(delivery.Payload, webhook.Secret);

                using var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url) { Content = content };
                request.Headers.Add("X-Webhook-Event", delivery.EventType);
                request.Headers.Add("X-Webhook-Delivery", delivery.Id.ToString());
                request.Headers.Add("X-Webhook-Signature", $"sha256={signature}");

                using var response = await HttpClient.SendAsync(request, ct);

                var responseBody = await response.Content.ReadAsStringAsync(ct);
                if (response.IsSuccessStatusCode)
                {
                    delivery.RecordSuccess((int)response.StatusCode, responseBody, now);
                    logger.LogInformation("Delivered webhook {DeliveryId} to {Url}", delivery.Id, webhook.Url);
                }
                else
                {
                    delivery.RecordFailure((int)response.StatusCode, responseBody, now, MaxAttempts);
                    logger.LogWarning("Webhook {DeliveryId} failed with HTTP {Status}", delivery.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                delivery.RecordFailure(null, ex.Message, now, MaxAttempts);
                logger.LogWarning(ex, "Webhook {DeliveryId} delivery threw an exception.", delivery.Id);
            }

            await webhookRepo.UpdateDeliveryAsync(delivery, ct);
        }
    }

    private static string ComputeSignature(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        return Convert.ToHexString(HMACSHA256.HashData(keyBytes, payloadBytes)).ToLowerInvariant();
    }
}
