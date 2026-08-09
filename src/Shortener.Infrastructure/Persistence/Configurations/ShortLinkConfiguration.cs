using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.Persistence.Configurations;

internal sealed class ShortLinkConfiguration : IEntityTypeConfiguration<ShortLink>
{
    public void Configure(EntityTypeBuilder<ShortLink> builder)
    {
        builder.ToTable("short_links");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.TenantId).HasColumnName("tenant_id");
        builder.Property(l => l.DomainId).HasColumnName("domain_id");
        builder.Property(l => l.ShortCode).HasColumnName("short_code").HasMaxLength(100).IsRequired();
        builder.Property(l => l.DestinationUrl).HasColumnName("destination_url").HasMaxLength(2048).IsRequired();
        builder.Property(l => l.Title).HasColumnName("title").HasMaxLength(500);
        builder.Property(l => l.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(l => l.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(l => l.CreatedBy).HasColumnName("created_by");
        builder.Property(l => l.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.Property(l => l.UpdatedBy).HasColumnName("updated_by");
        builder.Property(l => l.ExpiresAt).HasColumnName("expires_at");
        builder.Property(l => l.LastAccessedAt).HasColumnName("last_accessed_at");
        builder.Property(l => l.RedirectType).HasColumnName("redirect_type").HasConversion<int>();
        builder.Property(l => l.IsAnonymous).HasColumnName("is_anonymous");
        builder.Property(l => l.ClickCountSnapshot).HasColumnName("click_count_snapshot");
        builder.Property(l => l.ReportCount).HasColumnName("report_count");
        builder.Property(l => l.IsAbTest).HasColumnName("is_ab_test");
        builder.Property(l => l.HasGeoRoutes).HasColumnName("has_geo_routes");
        builder.Property(l => l.Version).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();

        builder.HasIndex(l => new { l.DomainId, l.ShortCode }).IsUnique()
            .HasDatabaseName("ix_short_links_domain_code");
        builder.HasIndex(l => new { l.TenantId, l.Status, l.CreatedAtUtc })
            .HasDatabaseName("ix_short_links_tenant_status_created");
        builder.HasIndex(l => l.ExpiresAt)
            .HasFilter("expires_at IS NOT NULL AND status = 'Active'")
            .HasDatabaseName("ix_short_links_expires_at_active");

        builder.HasOne<TenantDomain>().WithMany().HasForeignKey(l => l.DomainId).OnDelete(DeleteBehavior.Restrict);
    }
}
