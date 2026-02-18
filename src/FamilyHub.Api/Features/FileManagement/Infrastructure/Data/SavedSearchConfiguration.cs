using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class SavedSearchConfiguration : IEntityTypeConfiguration<SavedSearch>
{
    public void Configure(EntityTypeBuilder<SavedSearch> builder)
    {
        builder.ToTable("saved_searches", "file_management");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Query)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.FiltersJson)
            .HasColumnType("jsonb");

        builder.HasIndex(s => s.UserId);
    }
}
