using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Data;

public class FileThumbnailConfiguration : IEntityTypeConfiguration<FileThumbnail>
{
    public void Configure(EntityTypeBuilder<FileThumbnail> builder)
    {
        builder.ToTable("file_thumbnails", "file_management");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => FileThumbnailId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(t => t.FileId)
            .HasConversion(id => id.Value, value => FileId.From(value))
            .IsRequired();

        builder.Property(t => t.Width).IsRequired();
        builder.Property(t => t.Height).IsRequired();

        builder.Property(t => t.StorageKey)
            .HasConversion(k => k.Value, value => StorageKey.From(value))
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.GeneratedAt).IsRequired();

        builder.HasIndex(t => t.FileId);
        builder.HasIndex(t => new { t.FileId, t.Width, t.Height }).IsUnique();
    }
}
