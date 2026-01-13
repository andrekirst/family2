using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Modules.Family.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the FamilyMemberInvitation aggregate.
///
/// PHASE 5 STATE: Table resides in "family" schema after migration from "auth" schema.
///
/// CROSS-SCHEMA REFERENCES:
/// - InvitedByUserId references auth.users.id but without FK constraint
/// - Consistency maintained via application-level validation (IUserLookupService)
/// - FamilyId has FK constraint (same schema)
/// </summary>
public class FamilyMemberInvitationConfiguration : IEntityTypeConfiguration<FamilyMemberInvitation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FamilyMemberInvitation> builder)
    {
        // Table in "family" schema
        builder.ToTable("family_member_invitations", "family");

        // Primary key with Vogen value converter
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasConversion(new InvitationId.EfCoreValueConverter())
            .HasColumnName("invitation_id")
            .IsRequired();

        // Display code with Vogen value converter
        builder.Property(i => i.DisplayCode)
            .HasConversion(new InvitationDisplayCode.EfCoreValueConverter())
            .HasColumnName("display_code")
            .HasMaxLength(8)
            .IsRequired();

        // Family ID with Vogen value converter
        builder.Property(i => i.FamilyId)
            .HasConversion(new FamilyId.EfCoreValueConverter())
            .HasColumnName("family_id")
            .IsRequired();

        builder.HasIndex(i => i.FamilyId)
            .HasDatabaseName("ix_family_member_invitations_family_id");

        // Foreign key relationship to Family aggregate (same schema - FK constraint maintained)
        builder.HasOne<FamilyAggregate>()
            .WithMany()
            .HasForeignKey(i => i.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Email with Vogen value converter
        builder.Property(i => i.Email)
            .HasConversion(new Email.EfCoreValueConverter())
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        // Role (stored as varchar)
        builder.Property(i => i.Role)
            .HasConversion(
                role => role.Value,
                value => FamilyRole.From(value))
            .HasColumnName("role")
            .HasMaxLength(20)
            .IsRequired();

        // Token with Vogen value converter
        builder.Property(i => i.Token)
            .HasConversion(new InvitationToken.EfCoreValueConverter())
            .HasColumnName("token")
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(i => i.Token)
            .IsUnique()
            .HasDatabaseName("ix_family_member_invitations_token");

        // Expires at
        builder.Property(i => i.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(i => i.ExpiresAt)
            .HasDatabaseName("ix_family_member_invitations_expires_at");

        // Invited by user ID with Vogen value converter
        // NOTE: No FK constraint - cross-schema reference to auth.users
        // Consistency maintained via IUserLookupService
        builder.Property(i => i.InvitedByUserId)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("invited_by_user_id")
            .IsRequired();

        builder.HasIndex(i => i.InvitedByUserId)
            .HasDatabaseName("ix_family_member_invitations_invited_by_user_id");

        // Status with Vogen value converter
        builder.Property(i => i.Status)
            .HasConversion(new InvitationStatus.EfCoreValueConverter())
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        // Composite index on (family_id, status) for dashboard queries
        builder.HasIndex(i => new { i.FamilyId, i.Status })
            .HasDatabaseName("ix_family_member_invitations_family_id_status");

        // Message (optional personal note)
        builder.Property(i => i.Message)
            .HasColumnName("message")
            .HasColumnType("text")
            .IsRequired(false);

        // Audit fields
        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(i => i.AcceptedAt)
            .HasColumnName("accepted_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Ignore domain events collection
        builder.Ignore(i => i.DomainEvents);
    }
}
