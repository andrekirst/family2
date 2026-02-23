using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class FileTagConfiguration : IEntityTypeConfiguration<FileTag>
{
    public void Configure(EntityTypeBuilder<FileTag> builder)
    {
        builder.ToTable("file_tags", "file_management");

        builder.HasKey(ft => new { ft.FileId, ft.TagId });

        builder.Property(ft => ft.FileId)
            .HasConversion(
                id => id.Value,
                value => FileId.From(value));

        builder.Property(ft => ft.TagId)
            .HasConversion(
                id => id.Value,
                value => TagId.From(value));

        builder.Property(ft => ft.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
    }
}
