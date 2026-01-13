using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Modules.Auth.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the OutboxEvent entity.
/// </summary>
public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.ToTable("outbox_events", "auth");

        // Primary key with Vogen value converter
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(new OutboxEventId.EfCoreValueConverter())
            .HasColumnName("event_id")
            .IsRequired();

        // Event type (fully qualified class name)
        builder.Property(e => e.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(255)
            .IsRequired();

        // Event version for schema evolution
        builder.Property(e => e.EventVersion)
            .HasColumnName("event_version")
            .IsRequired();

        // Aggregate type (e.g., "FamilyMemberInvitation", "User")
        builder.Property(e => e.AggregateType)
            .HasColumnName("aggregate_type")
            .HasMaxLength(255)
            .IsRequired();

        // Aggregate ID
        builder.Property(e => e.AggregateId)
            .HasColumnName("aggregate_id")
            .IsRequired();

        // Payload (JSON)
        builder.Property(e => e.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        // Processing timestamp
        builder.Property(e => e.ProcessedAt)
            .HasColumnName("processed_at")
            .IsRequired(false);

        // Status enum
        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        // Retry count
        builder.Property(e => e.RetryCount)
            .HasColumnName("retry_count")
            .HasDefaultValue(0)
            .IsRequired();

        // Error message
        builder.Property(e => e.ErrorMessage)
            .HasColumnName("error_message")
            .HasColumnType("text")
            .IsRequired(false);

        // Audit fields (inherited from Entity)
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Composite index for worker queries (pending events ordered by creation time)
        builder.HasIndex(e => new { e.Status, e.CreatedAt })
            .HasDatabaseName("ix_outbox_events_status_created_at");

        // Index for cleanup queries
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("ix_outbox_events_created_at");

        // Note: OutboxEvent inherits from Entity<T>, not AggregateRoot<T>,
        // so it does not have a DomainEvents collection to ignore.
    }
}
