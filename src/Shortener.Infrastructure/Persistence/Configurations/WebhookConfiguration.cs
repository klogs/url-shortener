using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.Persistence.Configurations;

internal sealed class WebhookConfiguration : IEntityTypeConfiguration<Webhook>
{
    public void Configure(EntityTypeBuilder<Webhook> builder)
    {
        builder.ToTable("webhooks");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasColumnName("id");
        builder.Property(w => w.TenantId).HasColumnName("tenant_id");
        builder.Property(w => w.Url).HasColumnName("url").HasMaxLength(2048).IsRequired();
        builder.Property(w => w.Secret).HasColumnName("secret").HasMaxLength(100).IsRequired();
        builder.Property(w => w.Events).HasColumnName("events").HasMaxLength(500).IsRequired();
        builder.Property(w => w.IsActive).HasColumnName("is_active");
        builder.Property(w => w.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.HasIndex(w => w.TenantId).HasDatabaseName("ix_webhooks_tenant_id");

        builder.HasOne<Tenant>().WithMany().HasForeignKey(w => w.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
