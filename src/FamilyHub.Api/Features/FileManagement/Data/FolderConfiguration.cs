using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Data;

public class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.ToTable("folders", "file_management");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasConversion(id => id.Value, value => FolderId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(f => f.Name)
            .HasConversion(n => n.Value, value => FileName.From(value))
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(f => f.ParentFolderId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? FolderId.From(value.Value) : null)
            .IsRequired(false);

        builder.Property(f => f.MaterializedPath)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(f => f.FamilyId)
            .HasConversion(id => id.Value, value => FamilyId.From(value))
            .IsRequired();

        builder.Property(f => f.CreatedBy)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(f => f.CreatedAt).IsRequired();
        builder.Property(f => f.UpdatedAt).IsRequired();

        builder.HasIndex(f => f.FamilyId);
        builder.HasIndex(f => f.ParentFolderId);
        builder.HasIndex(f => f.MaterializedPath);
    }
}
