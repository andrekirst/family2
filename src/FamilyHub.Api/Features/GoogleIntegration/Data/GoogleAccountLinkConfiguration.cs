using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.GoogleIntegration.Data;

public class GoogleAccountLinkConfiguration : IEntityTypeConfiguration<GoogleAccountLink>
{
    public void Configure(EntityTypeBuilder<GoogleAccountLink> builder)
    {
        builder.ToTable("google_account_links", "google_integration");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => GoogleAccountLinkId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(e => e.GoogleAccountId)
            .HasConversion(id => id.Value, value => GoogleAccountId.From(value))
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.GoogleEmail)
            .HasConversion(e => e.Value, value => Email.From(value))
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(e => e.EncryptedAccessToken)
            .HasConversion(t => t.Value, value => EncryptedToken.From(value))
            .IsRequired();

        builder.Property(e => e.EncryptedRefreshToken)
            .HasConversion(t => t.Value, value => EncryptedToken.From(value))
            .IsRequired();

        builder.Property(e => e.AccessTokenExpiresAt)
            .IsRequired();

        builder.Property(e => e.GrantedScopes)
            .HasConversion(s => s.Value, value => GoogleScopes.From(value))
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion(s => s.Value, value => GoogleLinkStatus.From(value))
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(GoogleLinkStatus.From("Active"));

        builder.Property(e => e.LastSyncAt);
        builder.Property(e => e.LastError);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasIndex(e => e.GoogleAccountId).IsUnique();
        builder.HasIndex(e => e.Status);

        builder.Ignore(e => e.DomainEvents);
    }
}
