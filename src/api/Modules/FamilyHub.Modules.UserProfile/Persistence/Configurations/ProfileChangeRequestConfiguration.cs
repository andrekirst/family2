using FamilyHub.Modules.UserProfile.Domain.Aggregates;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Modules.UserProfile.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the ProfileChangeRequest entity.
/// Configures the profile_change_requests table in the user_profile schema.
/// </summary>
public class ProfileChangeRequestConfiguration : IEntityTypeConfiguration<ProfileChangeRequest>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProfileChangeRequest> builder)
    {
        builder.ToTable("profile_change_requests", "user_profile");

        // Primary key with Vogen value converter
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(new ChangeRequestId.EfCoreValueConverter())
            .HasColumnName("id")
            .IsRequired();

        // Profile ID (reference to profiles table)
        builder.Property(r => r.ProfileId)
            .HasConversion(new UserProfileId.EfCoreValueConverter())
            .HasColumnName("profile_id")
            .IsRequired();

        // Requested By (user who made the request)
        builder.Property(r => r.RequestedBy)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("requested_by")
            .IsRequired();

        // Family ID (for querying approval queue)
        builder.Property(r => r.FamilyId)
            .HasConversion(new FamilyId.EfCoreValueConverter())
            .HasColumnName("family_id")
            .IsRequired();

        // Field name being changed
        builder.Property(r => r.FieldName)
            .HasColumnName("field_name")
            .HasMaxLength(100)
            .IsRequired();

        // Old value (nullable - may not have been set before)
        builder.Property(r => r.OldValue)
            .HasColumnName("old_value")
            .HasMaxLength(500)
            .IsRequired(false);

        // New value (the requested value)
        builder.Property(r => r.NewValue)
            .HasColumnName("new_value")
            .HasMaxLength(500)
            .IsRequired();

        // Status with Vogen value converter
        builder.Property(r => r.Status)
            .HasConversion(new ChangeRequestStatus.EfCoreValueConverter())
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasDefaultValue(ChangeRequestStatus.Pending)
            .IsRequired();

        // Reviewed By (nullable - set when approved/rejected)
        builder.Property(r => r.ReviewedBy)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("reviewed_by")
            .IsRequired(false);

        // Reviewed At (nullable - set when approved/rejected)
        builder.Property(r => r.ReviewedAt)
            .HasColumnName("reviewed_at")
            .IsRequired(false);

        // Rejection Reason (nullable - only set when rejected)
        builder.Property(r => r.RejectionReason)
            .HasColumnName("rejection_reason")
            .HasMaxLength(500)
            .IsRequired(false);

        // Audit fields (managed by TimestampInterceptor)
        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Indexes for efficient querying
        // Index for family approval queue (pending requests by family)
        builder.HasIndex(r => new { r.FamilyId, r.Status })
            .HasDatabaseName("ix_profile_change_requests_family_id_status");

        // Index for user's pending changes
        builder.HasIndex(r => new { r.RequestedBy, r.Status })
            .HasDatabaseName("ix_profile_change_requests_requested_by_status");

        // Index for checking existing pending request for a field
        builder.HasIndex(r => new { r.ProfileId, r.FieldName, r.Status })
            .HasDatabaseName("ix_profile_change_requests_profile_id_field_name_status");

        // Ignore domain events collection (handled by base class)
        builder.Ignore(r => r.DomainEvents);
    }
}
