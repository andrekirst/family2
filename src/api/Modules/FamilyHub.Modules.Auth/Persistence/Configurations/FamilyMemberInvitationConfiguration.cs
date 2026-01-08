using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Modules.Auth.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the FamilyMemberInvitation entity.
/// </summary>
public class FamilyMemberInvitationConfiguration : IEntityTypeConfiguration<FamilyHub.Modules.Family.Domain.FamilyMemberInvitation>
{
    public void Configure(EntityTypeBuilder<FamilyHub.Modules.Family.Domain.FamilyMemberInvitation> builder)
    {
        builder.ToTable("family_member_invitations", "auth");

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

        builder.HasOne<FamilyHub.Modules.Family.Domain.Family>()
            .WithMany()
            .HasForeignKey(i => i.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Email with Vogen value converter (required - email-only invitations)
        builder.Property(i => i.Email)
            .HasConversion(new Email.EfCoreValueConverter())
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        // Role (stored as varchar, not using Vogen for FamilyRole enum)
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
        builder.Property(i => i.InvitedByUserId)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("invited_by_user_id")
            .IsRequired();

        builder.HasIndex(i => i.InvitedByUserId)
            .HasDatabaseName("ix_family_member_invitations_invited_by_user_id");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(i => i.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

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

        // Ignore domain events collection (handled by base class)
        builder.Ignore(i => i.DomainEvents);
    }
}
