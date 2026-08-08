using Microsoft.EntityFrameworkCore;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.Persistence;

public sealed class ShortenerDbContext(DbContextOptions<ShortenerDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantDomain> Domains => Set<TenantDomain>();
    public DbSet<ShortLink> ShortLinks => Set<ShortLink>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<Webhook> Webhooks => Set<Webhook>();
    public DbSet<WebhookDelivery> WebhookDeliveries => Set<WebhookDelivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShortenerDbContext).Assembly);
    }
}
