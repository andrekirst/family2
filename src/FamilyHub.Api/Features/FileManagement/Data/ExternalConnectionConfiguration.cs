using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.FileManagement.Data;

public sealed class ExternalConnectionConfiguration : IEntityTypeConfiguration<ExternalConnection>
{
    public void Configure(EntityTypeBuilder<ExternalConnection> builder)
    {
        builder.ToTable("external_connections", "file_management");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion<ExternalConnectionId.EfCoreValueConverter>();

        builder.Property(c => c.FamilyId)
            .HasConversion<FamilyId.EfCoreValueConverter>()
            .IsRequired();

        builder.Property(c => c.ProviderType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.EncryptedAccessToken)
            .IsRequired();

        builder.Property(c => c.EncryptedRefreshToken);

        builder.Property(c => c.TokenExpiresAt);

        builder.Property(c => c.ConnectedBy)
            .HasConversion<UserId.EfCoreValueConverter>()
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.ConnectedAt)
            .IsRequired();

        builder.HasIndex(c => c.FamilyId);
        builder.HasIndex(c => new { c.FamilyId, c.ProviderType }).IsUnique();

        builder.Ignore(c => c.DomainEvents);
    }
}
