using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shortener.Domain.Entities;
using Shortener.Domain.Enums;

namespace Shortener.Infrastructure.Persistence.Configurations;

internal sealed class TenantDomainConfiguration : IEntityTypeConfiguration<TenantDomain>
{
    public void Configure(EntityTypeBuilder<TenantDomain> builder)
    {
        builder.ToTable("domains");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.TenantId).HasColumnName("tenant_id");
        builder.Property(d => d.Host).HasColumnName("host").HasMaxLength(253).IsRequired();
        builder.Property(d => d.NormalizedHost).HasColumnName("normalized_host").HasMaxLength(253).IsRequired();
        builder.Property(d => d.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.IsDefault).HasColumnName("is_default");
        builder.Property(d => d.VerificationToken).HasColumnName("verification_token").HasMaxLength(100);
        builder.Property(d => d.VerifiedAt).HasColumnName("verified_at");
        builder.Property(d => d.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.HasIndex(d => d.NormalizedHost).IsUnique().HasDatabaseName("ix_domains_normalized_host");
        builder.HasIndex(d => d.TenantId).HasDatabaseName("ix_domains_tenant_id");

        builder.HasOne<Tenant>().WithMany().HasForeignKey(d => d.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}
