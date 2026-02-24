using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Data;

public class FileVersionConfiguration : IEntityTypeConfiguration<FileVersion>
{
    public void Configure(EntityTypeBuilder<FileVersion> builder)
    {
        builder.ToTable("file_versions", "file_management");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .HasConversion(id => id.Value, value => FileVersionId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(v => v.FileId)
            .HasConversion(id => id.Value, value => FileId.From(value))
            .IsRequired();

        builder.Property(v => v.VersionNumber)
            .IsRequired();

        builder.Property(v => v.StorageKey)
            .HasConversion(k => k.Value, value => StorageKey.From(value))
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(v => v.FileSize)
            .HasConversion(s => s.Value, value => FileSize.From(value))
            .IsRequired();

        builder.Property(v => v.Checksum)
            .HasConversion(c => c.Value, value => Checksum.From(value))
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(v => v.UploadedBy)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(v => v.IsCurrent).IsRequired();
        builder.Property(v => v.UploadedAt).IsRequired();

        builder.HasIndex(v => v.FileId);
        builder.HasIndex(v => new { v.FileId, v.IsCurrent });
    }
}
