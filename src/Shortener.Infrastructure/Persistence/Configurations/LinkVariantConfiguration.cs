using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.Persistence.Configurations;

internal sealed class LinkVariantConfiguration : IEntityTypeConfiguration<LinkVariant>
{
    public void Configure(EntityTypeBuilder<LinkVariant> builder)
    {
        builder.ToTable("link_variants");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.LinkId).HasColumnName("link_id");
        builder.Property(v => v.TenantId).HasColumnName("tenant_id");
        builder.Property(v => v.Label).HasColumnName("label").HasMaxLength(100).IsRequired();
        builder.Property(v => v.DestinationUrl).HasColumnName("destination_url").HasMaxLength(2048).IsRequired();
        builder.Property(v => v.Weight).HasColumnName("weight");
        builder.Property(v => v.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.HasIndex(v => v.LinkId).HasDatabaseName("ix_link_variants_link_id");
        builder.HasIndex(v => new { v.TenantId, v.LinkId }).HasDatabaseName("ix_link_variants_tenant_link");
    }
}
