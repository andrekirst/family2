using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Common.Infrastructure.Avatar.Data;

/// <summary>
/// EF Core configuration for AvatarAggregate entity.
/// Maps to PostgreSQL avatar schema.
/// </summary>
public class AvatarConfiguration : IEntityTypeConfiguration<AvatarAggregate>
{
    public void Configure(EntityTypeBuilder<AvatarAggregate> builder)
    {
        builder.ToTable("avatars", "avatar");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(
                id => id.Value,
                value => AvatarId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(a => a.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.OriginalMimeType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(a => a.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.HasMany(a => a.Variants)
            .WithOne(v => v.Avatar)
            .HasForeignKey(v => v.AvatarId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
