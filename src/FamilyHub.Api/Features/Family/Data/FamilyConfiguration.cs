using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.Api.Features.Family.Data;

/// <summary>
/// EF Core configuration for Family entity
/// Maps to PostgreSQL family schema with RLS policies applied via migration
/// </summary>
public class FamilyConfiguration : IEntityTypeConfiguration<FamilyEntity>
{
    public void Configure(EntityTypeBuilder<FamilyEntity> builder)
    {
        // Table mapping to family schema
        builder.ToTable("families", "family");

        // Primary key with Vogen converter
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .ValueGeneratedOnAdd();

        // Name - required (Vogen value object)
        builder.Property(f => f.Name)
            .HasConversion(
                name => name.Value,
                value => FamilyName.From(value))
            .HasMaxLength(100)
            .IsRequired();

        // Owner - required (Vogen value object)
        builder.Property(f => f.OwnerId)
            .HasConversion(
                ownerId => ownerId.Value,
                value => UserId.From(value))
            .IsRequired();
        builder.HasIndex(f => f.OwnerId);

        // Metadata fields
        builder.Property(f => f.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(f => f.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Relationships
        builder.HasOne(f => f.Owner)
            .WithMany()
            .HasForeignKey(f => f.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Members)
            .WithOne(u => u.Family)
            .HasForeignKey(u => u.FamilyId);
    }
}
