using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class RecentSearchConfiguration : IEntityTypeConfiguration<RecentSearch>
{
    public void Configure(EntityTypeBuilder<RecentSearch> builder)
    {
        builder.ToTable("recent_searches", "file_management");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Query)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(r => new { r.UserId, r.SearchedAt });
    }
}
