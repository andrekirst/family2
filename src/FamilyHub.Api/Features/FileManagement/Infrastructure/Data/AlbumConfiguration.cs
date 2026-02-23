using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class AlbumConfiguration : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.ToTable("albums", "file_management");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(
                id => id.Value,
                value => AlbumId.From(value));

        builder.Property(a => a.Name)
            .HasConversion(
                name => name.Value,
                value => AlbumName.From(value))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.Property(a => a.CoverFileId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value.Value,
                value => value == null ? null : FileId.From(value.Value));

        builder.Property(a => a.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value));

        builder.Property(a => a.CreatedBy)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value));

        builder.HasIndex(a => a.FamilyId);
    }
}
