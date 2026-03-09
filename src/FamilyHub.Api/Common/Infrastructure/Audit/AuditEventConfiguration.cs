using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Common.Infrastructure.Audit;

/// <summary>
/// EF Core configuration for the AuditEvent entity.
/// Maps to the public.audit_events table created by migration 20260309000001.
/// </summary>
public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.EventType)
            .IsRequired();

        builder.Property(e => e.EventId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(e => e.OccurredAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(e => e.ActorUserId)
            .HasColumnType("uuid");

        builder.Property(e => e.CorrelationId);

        builder.Property(e => e.CausationId)
            .HasColumnType("uuid");

        builder.Property(e => e.EntityType);

        builder.Property(e => e.EntityId);

        builder.Property(e => e.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        // Indexes matching the migration
        builder.HasIndex(e => e.EventType)
            .HasDatabaseName("ix_audit_events_event_type");

        builder.HasIndex(e => e.ActorUserId)
            .HasDatabaseName("ix_audit_events_actor_user_id")
            .HasFilter("actor_user_id IS NOT NULL");

        builder.HasIndex(e => new { e.EntityType, e.EntityId })
            .HasDatabaseName("ix_audit_events_entity")
            .HasFilter("entity_type IS NOT NULL");

        builder.HasIndex(e => e.CorrelationId)
            .HasDatabaseName("ix_audit_events_correlation_id")
            .HasFilter("correlation_id IS NOT NULL");

        builder.HasIndex(e => e.OccurredAt)
            .HasDatabaseName("ix_audit_events_occurred_at");
    }
}
