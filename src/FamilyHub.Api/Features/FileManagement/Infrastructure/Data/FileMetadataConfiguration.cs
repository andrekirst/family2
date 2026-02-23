using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class FileMetadataConfiguration : IEntityTypeConfiguration<FileMetadata>
{
    public void Configure(EntityTypeBuilder<FileMetadata> builder)
    {
        builder.ToTable("file_metadata", "file_management");

        builder.HasKey(m => m.FileId);
        builder.Property(m => m.FileId)
            .HasConversion(
                id => id.Value,
                value => FileId.From(value));

        builder.Property(m => m.GpsLatitude)
            .HasConversion(
                lat => lat == null ? (double?)null : lat.Value.Value,
                value => value == null ? null : Latitude.From(value.Value));

        builder.Property(m => m.GpsLongitude)
            .HasConversion(
                lon => lon == null ? (double?)null : lon.Value.Value,
                value => value == null ? null : Longitude.From(value.Value));

        builder.Property(m => m.LocationName)
            .HasMaxLength(500);

        builder.Property(m => m.CameraModel)
            .HasMaxLength(200);

        builder.Property(m => m.RawExif)
            .HasColumnType("jsonb");
    }
}
