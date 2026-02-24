using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

public class UserFavoriteConfiguration : IEntityTypeConfiguration<UserFavorite>
{
    public void Configure(EntityTypeBuilder<UserFavorite> builder)
    {
        builder.ToTable("user_favorites", "file_management");

        builder.HasKey(uf => new { uf.UserId, uf.FileId });

        builder.Property(uf => uf.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value));

        builder.Property(uf => uf.FileId)
            .HasConversion(
                id => id.Value,
                value => FileId.From(value));

        builder.HasIndex(uf => uf.UserId);
        builder.HasIndex(uf => uf.FileId);
    }
}
