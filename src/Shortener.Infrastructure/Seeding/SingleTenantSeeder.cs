using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shortener.Application.Options;
using Shortener.Domain.Entities;
using Shortener.Infrastructure.Persistence;

namespace Shortener.Infrastructure.Seeding;

public sealed class SingleTenantSeeder(
    IServiceScopeFactory scopeFactory,
    IOptions<ShortenerOptions> opts,
    TimeProvider time) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!opts.Value.MultitenancyMode.Equals("SingleTenant", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var defaultDomain = opts.Value.DefaultDomain;
        if (string.IsNullOrWhiteSpace(defaultDomain))
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShortenerDbContext>();

        var normalizedHost = TenantDomain.NormalizeHost(defaultDomain);
        var exists = await db.Domains.AnyAsync(
            d => d.NormalizedHost == normalizedHost, cancellationToken);

        if (exists)
        {
            return;
        }

        var now = time.GetUtcNow();
        var tenant = Tenant.Create("Default", now);
        var domain = TenantDomain.CreateVerified(tenant.Id, defaultDomain, isDefault: true, now);

        db.Tenants.Add(tenant);
        db.Domains.Add(domain);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
