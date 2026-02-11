using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.Family.Data;

/// <summary>
/// EF Core configuration for FamilyMember entity.
/// Maps to PostgreSQL family schema with role tracking.
/// </summary>
public class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
    public void Configure(EntityTypeBuilder<FamilyMember> builder)
    {
        builder.ToTable("family_members", "family");

        builder.HasKey(fm => fm.Id);
        builder.Property(fm => fm.Id)
            .HasConversion(
                id => id.Value,
                value => FamilyMemberId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(fm => fm.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .IsRequired();

        builder.Property(fm => fm.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.Property(fm => fm.Role)
            .HasConversion(
                role => role.Value,
                value => FamilyRole.From(value))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(fm => fm.JoinedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(fm => fm.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Unique constraint: a user can only be a member of a family once
        builder.HasIndex(fm => new { fm.FamilyId, fm.UserId })
            .IsUnique();

        // Relationships
        builder.HasOne(fm => fm.Family)
            .WithMany(f => f.FamilyMembers)
            .HasForeignKey(fm => fm.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fm => fm.User)
            .WithMany()
            .HasForeignKey(fm => fm.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
