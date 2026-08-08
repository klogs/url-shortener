using System.Text.Json;
using Shortener.Application.Interfaces;

namespace Shortener.Worker;

public sealed class LinkExpirationSweepWorker(
    IServiceScopeFactory scopeFactory,
    TimeProvider time,
    ILogger<LinkExpirationSweepWorker> logger) : BackgroundService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private static readonly TimeSpan WarningWindow = TimeSpan.FromDays(7);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("LinkExpirationSweepWorker starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SweepAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "LinkExpirationSweepWorker encountered an error");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task SweepAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var links = scope.ServiceProvider.GetRequiredService<IShortLinkRepository>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IWebhookDispatcher>();

        var now = time.GetUtcNow();
        var expiringSoon = await links.ListExpiringSoonAsync(now, now.Add(WarningWindow), BatchSize, ct);

        if (expiringSoon.Count == 0)
        {
            return;
        }

        logger.LogInformation("LinkExpirationSweepWorker: dispatching expiry alerts for {Count} links", expiringSoon.Count);

        foreach (var link in expiringSoon)
        {
            var payload = JsonSerializer.Serialize(new
            {
                linkId = link.Id,
                shortCode = link.ShortCode,
                expiresAt = link.ExpiresAt,
                tenantId = link.TenantId,
            });

            await dispatcher.DispatchAsync(link.TenantId, "link.expiring_soon", payload, ct);
        }
    }
}
