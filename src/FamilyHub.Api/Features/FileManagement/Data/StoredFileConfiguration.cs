using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Data;

public class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("files", "file_management");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasConversion(id => id.Value, value => FileId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(f => f.Name)
            .HasConversion(n => n.Value, value => FileName.From(value))
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(f => f.MimeType)
            .HasConversion(m => m.Value, value => MimeType.From(value))
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(f => f.Size)
            .HasConversion(s => s.Value, value => FileSize.From(value))
            .IsRequired();

        builder.Property(f => f.StorageKey)
            .HasConversion(k => k.Value, value => StorageKey.From(value))
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(f => f.Checksum)
            .HasConversion(c => c.Value, value => Checksum.From(value))
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(f => f.FolderId)
            .HasConversion(id => id.Value, value => FolderId.From(value))
            .IsRequired();

        builder.Property(f => f.FamilyId)
            .HasConversion(id => id.Value, value => FamilyId.From(value))
            .IsRequired();

        builder.Property(f => f.UploadedBy)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(f => f.CreatedAt).IsRequired();
        builder.Property(f => f.UpdatedAt).IsRequired();

        builder.HasIndex(f => f.FolderId);
        builder.HasIndex(f => f.FamilyId);
    }
}
