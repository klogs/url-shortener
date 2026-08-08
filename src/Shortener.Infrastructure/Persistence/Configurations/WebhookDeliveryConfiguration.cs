using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shortener.Domain.Entities;
using Shortener.Domain.Enums;

namespace Shortener.Infrastructure.Persistence.Configurations;

internal sealed class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
    {
        builder.ToTable("webhook_deliveries");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.WebhookId).HasColumnName("webhook_id");
        builder.Property(d => d.TenantId).HasColumnName("tenant_id");
        builder.Property(d => d.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
        builder.Property(d => d.Payload).HasColumnName("payload").IsRequired();
        builder.Property(d => d.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.AttemptCount).HasColumnName("attempt_count");
        builder.Property(d => d.NextAttemptAt).HasColumnName("next_attempt_at");
        builder.Property(d => d.DeliveredAt).HasColumnName("delivered_at");
        builder.Property(d => d.LastResponseBody).HasColumnName("last_response_body");
        builder.Property(d => d.LastHttpStatus).HasColumnName("last_http_status");
        builder.Property(d => d.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.HasIndex(d => new { d.Status, d.NextAttemptAt })
            .HasFilter("status = 'Pending'")
            .HasDatabaseName("ix_webhook_deliveries_pending");
        builder.HasIndex(d => d.WebhookId).HasDatabaseName("ix_webhook_deliveries_webhook_id");

        builder.HasOne<Webhook>().WithMany().HasForeignKey(d => d.WebhookId).OnDelete(DeleteBehavior.Cascade);
    }
}
