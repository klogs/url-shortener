using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.Persistence.Configurations;

internal sealed class GeoRouteConfiguration : IEntityTypeConfiguration<GeoRoute>
{
    public void Configure(EntityTypeBuilder<GeoRoute> builder)
    {
        builder.ToTable("geo_routes");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.LinkId).HasColumnName("link_id");
        builder.Property(r => r.TenantId).HasColumnName("tenant_id");
        builder.Property(r => r.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
        builder.Property(r => r.DestinationUrl).HasColumnName("destination_url").HasMaxLength(2048).IsRequired();
        builder.Property(r => r.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.HasIndex(r => r.LinkId).HasDatabaseName("ix_geo_routes_link_id");
        builder.HasIndex(r => new { r.TenantId, r.LinkId }).HasDatabaseName("ix_geo_routes_tenant_link");
        builder.HasIndex(r => new { r.LinkId, r.CountryCode }).IsUnique()
            .HasDatabaseName("ix_geo_routes_link_country_unique");

        builder.HasOne<ShortLink>().WithMany().HasForeignKey(r => r.LinkId).OnDelete(DeleteBehavior.Cascade);
    }
}
