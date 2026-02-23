using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags", "file_management");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => TagId.From(value));

        builder.Property(t => t.Name)
            .HasConversion(
                name => name.Value,
                value => TagName.From(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Color)
            .HasConversion(
                color => color.Value,
                value => TagColor.From(value))
            .HasMaxLength(7)
            .IsRequired();

        builder.Property(t => t.FamilyId)
            .HasConversion(
                id => id.Value,
                value => FamilyId.From(value));

        builder.Property(t => t.CreatedBy)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value));

        builder.HasIndex(t => new { t.FamilyId, t.Name })
            .IsUnique();
    }
}
