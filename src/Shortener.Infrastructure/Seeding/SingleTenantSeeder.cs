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
        var existingDomain = await db.Domains
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.NormalizedHost == normalizedHost, cancellationToken);

        if (existingDomain is not null)
        {
            // Domain already exists. If DefaultTenantId is configured and differs from
            // the existing tenant, re-seed with the correct ID.
            var desiredId = opts.Value.DefaultTenantId;
            if (!desiredId.HasValue || existingDomain.TenantId == desiredId.Value)
            {
                return;
            }

            await ReseedWithNewTenantId(db, existingDomain.TenantId, desiredId.Value, defaultDomain, cancellationToken);
            return;
        }

        var tenantId = opts.Value.DefaultTenantId ?? Guid.NewGuid();
        var createdAt = time.GetUtcNow();
        db.Tenants.Add(Tenant.CreateWithId(tenantId, "Default", createdAt));
        db.Domains.Add(TenantDomain.CreateVerified(tenantId, defaultDomain, isDefault: true, createdAt));
        await db.SaveChangesAsync(cancellationToken);
    }

    // Drops all data for the old tenant and re-seeds with the desired ID.
    // FK schema: short_links→domains (RESTRICT), domains→tenants (RESTRICT),
    // all others (api_keys, webhooks, etc.) use CASCADE from tenants/webhooks.
    // Delete order must respect RESTRICT constraints: leaf entities first.
    private async Task ReseedWithNewTenantId(
        ShortenerDbContext db,
        Guid oldId,
        Guid newId,
        string defaultDomain,
        CancellationToken ct)
    {
        // Leaf nodes (depend on short_links via CASCADE, but deleting explicitly for safety)
        await db.GeoRoutes.Where(x => x.TenantId == oldId).ExecuteDeleteAsync(ct);
        await db.LinkVariants.Where(x => x.TenantId == oldId).ExecuteDeleteAsync(ct);
        await db.AbuseReports.Where(x => x.TenantId == oldId).ExecuteDeleteAsync(ct);

        // WebhookDeliveries cascade from Webhooks, but delete explicitly first
        await db.WebhookDeliveries.Where(x => x.TenantId == oldId).ExecuteDeleteAsync(ct);
        await db.Webhooks.Where(x => x.TenantId == oldId).ExecuteDeleteAsync(ct);

        // ShortLinks must go before Domains (RESTRICT FK)
        await db.ShortLinks.Where(x => x.TenantId == oldId).ExecuteDeleteAsync(ct);

        // ApiKeys cascade from Tenant, but delete explicitly before Tenant
        await db.ApiKeys.Where(x => x.TenantId == oldId).ExecuteDeleteAsync(ct);

        // Domains must go before Tenants (RESTRICT FK)
        await db.Domains.Where(x => x.TenantId == oldId).ExecuteDeleteAsync(ct);

        await db.Tenants.Where(t => t.Id == oldId).ExecuteDeleteAsync(ct);

        var now = time.GetUtcNow();
        db.Tenants.Add(Tenant.CreateWithId(newId, "Default", now));
        db.Domains.Add(TenantDomain.CreateVerified(newId, defaultDomain, isDefault: true, now));
        await db.SaveChangesAsync(ct);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
