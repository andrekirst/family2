using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Common.Infrastructure.Avatar.Data;

/// <summary>
/// EF Core configuration for AvatarVariant entity.
/// </summary>
public class AvatarVariantConfiguration : IEntityTypeConfiguration<AvatarVariant>
{
    public void Configure(EntityTypeBuilder<AvatarVariant> builder)
    {
        builder.ToTable("avatar_variants", "avatar");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedOnAdd();

        builder.Property(v => v.AvatarId)
            .HasConversion(
                id => id.Value,
                value => AvatarId.From(value))
            .IsRequired();

        builder.Property(v => v.Size)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(v => v.StorageKey)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(v => v.MimeType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(v => v.FileSize).IsRequired();
        builder.Property(v => v.Width).IsRequired();
        builder.Property(v => v.Height).IsRequired();

        // Unique: one variant per size per avatar
        builder.HasIndex(v => new { v.AvatarId, v.Size }).IsUnique();
    }
}
