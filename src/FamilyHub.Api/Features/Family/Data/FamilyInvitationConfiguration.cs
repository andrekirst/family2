using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.Family.Data;

/// <summary>
/// EF Core configuration for FamilyInvitation aggregate.
/// Maps to PostgreSQL family schema with token hash indexing.
/// </summary>
public class FamilyInvitationConfiguration : IEntityTypeConfiguration<FamilyInvitation>
{
    public void Configure(EntityTypeBuilder<FamilyInvitation> builder)
    {
        builder.ToTable("family_invitations", "family");

        builder.HasKey(fi => fi.Id);
        builder.Property(fi => fi.Id)
            .HasConversion(
                id => id.Value,
                value => InvitationId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(fi => fi.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .IsRequired();

        builder.Property(fi => fi.InvitedByUserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.Property(fi => fi.InviteeEmail)
            .HasConversion(
                email => email.Value,
                value => Email.From(value))
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(fi => fi.TokenHash)
            .HasConversion(
                token => token.Value,
                value => InvitationToken.From(value))
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(fi => fi.Role)
            .HasConversion(
                role => role.Value,
                value => FamilyRole.From(value))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(fi => fi.Status)
            .HasConversion(
                status => status.Value,
                value => InvitationStatus.From(value))
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(InvitationStatus.Pending);

        builder.Property(fi => fi.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(fi => fi.ExpiresAt)
            .IsRequired();

        builder.Property(fi => fi.AcceptedByUserId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? UserId.From(value.Value) : null)
            .IsRequired(false);

        builder.Property(fi => fi.AcceptedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(fi => fi.TokenHash)
            .IsUnique();

        builder.HasIndex(fi => new { fi.FamilyId, fi.Status });

        // Relationships
        builder.HasOne(fi => fi.Family)
            .WithMany()
            .HasForeignKey(fi => fi.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fi => fi.InvitedByUser)
            .WithMany()
            .HasForeignKey(fi => fi.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(fi => fi.AcceptedByUser)
            .WithMany()
            .HasForeignKey(fi => fi.AcceptedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
