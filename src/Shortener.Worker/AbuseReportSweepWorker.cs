using Microsoft.Extensions.Options;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;
using Shortener.Domain.Entities;

namespace Shortener.Worker;

public sealed class AbuseReportSweepWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<AbuseOptions> abuseOptions,
    TimeProvider time,
    ILogger<AbuseReportSweepWorker> logger) : BackgroundService
{
    private const int BatchSize = 50;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("AbuseReportSweepWorker starting");

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
                logger.LogError(ex, "AbuseReportSweepWorker encountered an error");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task SweepAsync(CancellationToken ct)
    {
        var threshold = abuseOptions.Value.AutoBlockThreshold;

        await using var scope = scopeFactory.CreateAsyncScope();
        var links = scope.ServiceProvider.GetRequiredService<IShortLinkRepository>();
        var domains = scope.ServiceProvider.GetRequiredService<IDomainRepository>();
        var cache = scope.ServiceProvider.GetRequiredService<IRedirectCache>();

        var candidates = await links.ListAboveReportThresholdAsync(threshold, BatchSize, ct);
        if (candidates.Count == 0)
        {
            return;
        }

        logger.LogInformation("AbuseReportSweepWorker: auto-blocking {Count} links", candidates.Count);

        var now = time.GetUtcNow();
        var domainLookup = new Dictionary<Guid, TenantDomain?>();

        foreach (var link in candidates)
        {
            if (!domainLookup.TryGetValue(link.DomainId, out var domain))
            {
                domain = await domains.GetByIdAsync(link.DomainId, link.TenantId, ct);
                domainLookup[link.DomainId] = domain;
            }

            link.AutoBlock(now);
            await links.UpdateAsync(link, ct);

            if (domain is not null)
            {
                await cache.RemoveAsync(domain.NormalizedHost, link.ShortCode, ct);
            }
        }
    }
}
