using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

/// <summary>
/// EF Core configuration for FileBlob entity.
/// Stores binary file data in the file_management schema.
/// </summary>
public class FileBlobConfiguration : IEntityTypeConfiguration<FileBlob>
{
    public void Configure(EntityTypeBuilder<FileBlob> builder)
    {
        builder.ToTable("file_blobs", "file_management");

        builder.HasKey(f => f.StorageKey);
        builder.Property(f => f.StorageKey)
            .HasMaxLength(255);

        builder.Property(f => f.Data)
            .IsRequired();

        builder.Property(f => f.MimeType)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(f => f.Size)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
    }
}
