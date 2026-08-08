using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shortener.Domain.Entities;

namespace Shortener.Infrastructure.Persistence.Configurations;

internal sealed class AbuseReportConfiguration : IEntityTypeConfiguration<AbuseReport>
{
    public void Configure(EntityTypeBuilder<AbuseReport> builder)
    {
        builder.ToTable("abuse_reports");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.LinkId).HasColumnName("link_id");
        builder.Property(r => r.TenantId).HasColumnName("tenant_id");
        builder.Property(r => r.ShortCode).HasColumnName("short_code").HasMaxLength(100).IsRequired();
        builder.Property(r => r.NormalizedHost).HasColumnName("normalized_host").HasMaxLength(253).IsRequired();
        builder.Property(r => r.ReporterIp).HasColumnName("reporter_ip").HasMaxLength(45);
        builder.Property(r => r.Reason).HasColumnName("reason").HasMaxLength(500);
        builder.Property(r => r.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.HasIndex(r => r.LinkId).HasDatabaseName("ix_abuse_reports_link_id");
        builder.HasIndex(r => r.CreatedAtUtc).HasDatabaseName("ix_abuse_reports_created_at");
    }
}
