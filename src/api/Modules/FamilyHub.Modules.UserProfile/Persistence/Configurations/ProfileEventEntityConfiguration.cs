using FamilyHub.Modules.UserProfile.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Modules.UserProfile.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the ProfileEventEntity.
/// Configures the profile_events table in the user_profile schema.
/// </summary>
public class ProfileEventEntityConfiguration : IEntityTypeConfiguration<ProfileEventEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProfileEventEntity> builder)
    {
        builder.ToTable("profile_events", "user_profile");

        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();

        // Profile ID (foreign key to profiles table)
        builder.Property(e => e.ProfileId)
            .HasColumnName("profile_id")
            .IsRequired();

        // Event Type (for deserialization)
        builder.Property(e => e.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        // Event Data (JSONB for efficient querying in PostgreSQL)
        builder.Property(e => e.EventData)
            .HasColumnName("event_data")
            .HasColumnType("jsonb")
            .IsRequired();

        // Changed By (user who made the change)
        builder.Property(e => e.ChangedBy)
            .HasColumnName("changed_by")
            .IsRequired();

        // Occurred At (when the event happened)
        builder.Property(e => e.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        // Version (sequential, unique per profile)
        builder.Property(e => e.Version)
            .HasColumnName("version")
            .IsRequired();

        // Unique index on (profile_id, version) for event ordering and optimistic concurrency
        builder.HasIndex(e => new { e.ProfileId, e.Version })
            .HasDatabaseName("ix_profile_events_profile_version")
            .IsUnique();

        // Index on (profile_id, event_type) for efficient snapshot retrieval
        builder.HasIndex(e => new { e.ProfileId, e.EventType })
            .HasDatabaseName("ix_profile_events_profile_type");

        // Index on occurred_at for time-based queries
        builder.HasIndex(e => e.OccurredAt)
            .HasDatabaseName("ix_profile_events_occurred_at");

        // Note: Foreign key constraint is defined via migration SQL
        // EF Core navigation is not used due to Vogen UserProfileId incompatibility
        // The FK constraint will be: profile_id -> profiles.id with CASCADE DELETE
    }
}
