using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class FilePermissionConfiguration : IEntityTypeConfiguration<FilePermission>
{
    public void Configure(EntityTypeBuilder<FilePermission> builder)
    {
        builder.ToTable("file_permissions", "file_management");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ResourceType)
            .IsRequired();

        builder.Property(p => p.ResourceId)
            .IsRequired();

        builder.Property(p => p.PermissionLevel)
            .IsRequired();

        builder.HasIndex(p => new { p.ResourceType, p.ResourceId });
        builder.HasIndex(p => new { p.MemberId, p.FamilyId });
        builder.HasIndex(p => new { p.MemberId, p.ResourceType, p.ResourceId }).IsUnique();
    }
}
