using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Domain.Entities;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.Photos.Data;

public class PhotoConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder.ToTable("photos", "photos");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => PhotoId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(e => e.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value))
            .IsRequired();

        builder.Property(e => e.UploadedBy)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.Property(e => e.FileName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.FileSizeBytes)
            .IsRequired();

        builder.Property(e => e.StoragePath)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(e => e.Caption)
            .HasConversion(
                caption => caption.HasValue ? caption.Value.Value : null,
                value => value != null ? PhotoCaption.From(value) : (PhotoCaption?)null)
            .HasMaxLength(500);

        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Indexes for efficient grid pagination and filtered queries
        builder.HasIndex(e => new { e.FamilyId, e.CreatedAt });
        builder.HasIndex(e => new { e.FamilyId, e.IsDeleted });

        // Ignore domain events — not persisted
        builder.Ignore(e => e.DomainEvents);
    }
}
