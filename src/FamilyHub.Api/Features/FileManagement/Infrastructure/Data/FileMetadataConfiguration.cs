using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class FileMetadataConfiguration : IEntityTypeConfiguration<FileMetadata>
{
    public void Configure(EntityTypeBuilder<FileMetadata> builder)
    {
        builder.ToTable("file_metadata", "file_management");

        builder.HasKey(m => m.FileId);

        builder.Property(m => m.LocationName)
            .HasMaxLength(500);

        builder.Property(m => m.CameraModel)
            .HasMaxLength(200);

        builder.Property(m => m.RawExif)
            .HasColumnType("jsonb");
    }
}
