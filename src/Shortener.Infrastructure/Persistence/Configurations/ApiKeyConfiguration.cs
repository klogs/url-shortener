using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.Persistence.Configurations;

internal sealed class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");
        builder.HasKey(k => k.Id);
        builder.Property(k => k.Id).HasColumnName("id");
        builder.Property(k => k.TenantId).HasColumnName("tenant_id");
        builder.Property(k => k.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(k => k.KeyPrefix).HasColumnName("key_prefix").HasMaxLength(8).IsRequired();
        builder.Property(k => k.KeyHash).HasColumnName("key_hash").HasMaxLength(64).IsRequired();
        builder.Property(k => k.Scopes).HasColumnName("scopes").HasMaxLength(500).IsRequired();
        builder.Property(k => k.ExpiresAt).HasColumnName("expires_at");
        builder.Property(k => k.IsRevoked).HasColumnName("is_revoked");
        builder.Property(k => k.LastUsedAtUtc).HasColumnName("last_used_at_utc");
        builder.Property(k => k.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.HasIndex(k => k.KeyPrefix).HasDatabaseName("ix_api_keys_prefix");
        builder.HasIndex(k => k.TenantId).HasDatabaseName("ix_api_keys_tenant_id");

        builder.HasOne<Tenant>().WithMany().HasForeignKey(k => k.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
