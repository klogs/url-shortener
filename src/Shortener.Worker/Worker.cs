namespace Shortener.Worker;

// Placeholder — concrete background services will be added in Phase 1/2.
// Each background job will be its own IHostedService implementation.
internal sealed class PlaceholderWorker : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.CompletedTask;
}
