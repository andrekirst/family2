using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Api.Features.GoogleIntegration.Data;

public class OAuthStateConfiguration : IEntityTypeConfiguration<OAuthState>
{
    public void Configure(EntityTypeBuilder<OAuthState> builder)
    {
        builder.ToTable("oauth_states", "google_integration");

        builder.HasKey(e => e.State);
        builder.Property(e => e.State)
            .HasMaxLength(128);

        builder.Property(e => e.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(e => e.CodeVerifier)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.ExpiresAt)
            .IsRequired();

        builder.HasIndex(e => e.ExpiresAt);
    }
}
