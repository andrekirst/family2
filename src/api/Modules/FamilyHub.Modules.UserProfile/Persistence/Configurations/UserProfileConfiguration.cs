using FamilyHub.Modules.UserProfile.Domain.Aggregates;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Modules.UserProfile.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the UserProfile entity.
/// Configures the profiles table in the user_profile schema with owned entities.
/// </summary>
public class UserProfileConfiguration : IEntityTypeConfiguration<Domain.Aggregates.UserProfile>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Domain.Aggregates.UserProfile> builder)
    {
        builder.ToTable("profiles", "user_profile");

        // Primary key with Vogen value converter
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(new UserProfileId.EfCoreValueConverter())
            .HasColumnName("id")
            .IsRequired();

        // User ID (cross-schema reference to auth.users, no FK constraint)
        builder.Property(p => p.UserId)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(p => p.UserId)
            .IsUnique()
            .HasDatabaseName("ix_profiles_user_id");

        // Display Name with Vogen value converter
        builder.Property(p => p.DisplayName)
            .HasConversion(new DisplayName.EfCoreValueConverter())
            .HasColumnName("display_name")
            .HasMaxLength(100)
            .IsRequired();

        // Birthday (nullable) with Vogen value converter
        builder.Property(p => p.Birthday)
            .HasConversion(new Birthday.EfCoreValueConverter())
            .HasColumnName("birthday")
            .IsRequired(false);

        // Pronouns (nullable) with Vogen value converter
        builder.Property(p => p.Pronouns)
            .HasConversion(new Pronouns.EfCoreValueConverter())
            .HasColumnName("pronouns")
            .HasMaxLength(50)
            .IsRequired(false);

        // Owned entity: ProfilePreferences (stored as columns in profiles table)
        builder.OwnsOne(p => p.Preferences, prefs =>
        {
            prefs.Property(pr => pr.Language)
                .HasColumnName("language")
                .HasMaxLength(ProfilePreferences.LanguageMaxLength)
                .HasDefaultValue(ProfilePreferences.DefaultLanguage)
                .IsRequired();

            prefs.Property(pr => pr.Timezone)
                .HasColumnName("timezone")
                .HasMaxLength(ProfilePreferences.TimezoneMaxLength)
                .HasDefaultValue(ProfilePreferences.DefaultTimezone)
                .IsRequired();

            prefs.Property(pr => pr.DateFormat)
                .HasColumnName("date_format")
                .HasMaxLength(ProfilePreferences.DateFormatMaxLength)
                .HasDefaultValue(ProfilePreferences.DefaultDateFormat)
                .IsRequired();
        });

        // Owned entity: ProfileFieldVisibility (stored as columns in profiles table)
        builder.OwnsOne(p => p.FieldVisibility, vis =>
        {
            vis.Property(v => v.BirthdayVisibility)
                .HasConversion(new VisibilityLevel.EfCoreValueConverter())
                .HasColumnName("birthday_visibility")
                .HasMaxLength(20)
                .HasDefaultValue(VisibilityLevel.Family)
                .IsRequired();

            vis.Property(v => v.PronounsVisibility)
                .HasConversion(new VisibilityLevel.EfCoreValueConverter())
                .HasColumnName("pronouns_visibility")
                .HasMaxLength(20)
                .HasDefaultValue(VisibilityLevel.Family)
                .IsRequired();

            vis.Property(v => v.PreferencesVisibility)
                .HasConversion(new VisibilityLevel.EfCoreValueConverter())
                .HasColumnName("preferences_visibility")
                .HasMaxLength(20)
                .HasDefaultValue(VisibilityLevel.Hidden)
                .IsRequired();
        });

        // Zitadel sync status tracking
        builder.Property(p => p.SyncStatus)
            .HasConversion(new SyncStatus.EfCoreValueConverter())
            .HasColumnName("sync_status")
            .HasMaxLength(20)
            .HasDefaultValue(SyncStatus.Pending)
            .IsRequired();

        // Last synced timestamp (nullable - null means never synced)
        builder.Property(p => p.LastSyncedAt)
            .HasColumnName("last_synced_at")
            .IsRequired(false);

        // Audit fields (managed by TimestampInterceptor)
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Ignore domain events collection (handled by base class)
        builder.Ignore(p => p.DomainEvents);
    }
}
