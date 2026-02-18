using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class FileTagConfiguration : IEntityTypeConfiguration<FileTag>
{
    public void Configure(EntityTypeBuilder<FileTag> builder)
    {
        builder.ToTable("file_tags", "file_management");

        builder.HasKey(ft => new { ft.FileId, ft.TagId });

        builder.Property(ft => ft.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
    }
}
