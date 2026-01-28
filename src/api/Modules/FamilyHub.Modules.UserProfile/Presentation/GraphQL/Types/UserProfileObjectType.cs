using FamilyHub.Infrastructure.GraphQL.Directives;
using FamilyHub.Infrastructure.GraphQL.Types;
using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Presentation.GraphQL.Relay;
using HotChocolate;
using HotChocolate.Types;
using UserProfileAggregate = FamilyHub.Modules.UserProfile.Domain.Aggregates.UserProfile;

namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL ObjectType configuration for the UserProfile entity.
/// Implements the Relay Node interface for global ID resolution.
/// </summary>
/// <remarks>
/// <para>
/// This type:
/// <list type="bullet">
/// <item><description>Exposes UserProfile entity as a GraphQL "UserProfile" type</description></item>
/// <item><description>Implements Node interface with base64-encoded global IDs</description></item>
/// <item><description>Provides node resolution via repository lookup</description></item>
/// </list>
/// </para>
/// <para>
/// The global ID format is: base64("UserProfile:{guid}")
/// </para>
/// <para>
/// Field-level visibility is enforced via the @visible directive which specifies
/// the default visibility level. The VisibilityFieldMiddleware checks viewer
/// relationships at runtime to determine field access.
/// </para>
/// </remarks>
public sealed class UserProfileObjectType : ObjectType<UserProfileAggregate>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<UserProfileAggregate> descriptor)
    {
        descriptor.Name("UserProfile");
        descriptor.Description("A user's profile containing personal information and preferences.");

        descriptor.BindFieldsExplicitly();

        // Implement Relay Node interface
        descriptor
            .ImplementsNode()
            .IdField(p => p.Id.Value)
            .ResolveNode(async (ctx, id) =>
            {
                var repository = ctx.Service<IUserProfileRepository>();
                return await repository.GetByIdAsync(UserProfileId.From(id), ctx.RequestAborted);
            });

        // Override the ID field to return global ID
        descriptor
            .Field("id")
            .Type<NonNullType<IdType>>()
            .Description("Global ID (Relay Node specification)")
            .Resolve(ctx => GlobalIdSerializer.Serialize("UserProfile", ctx.Parent<UserProfileAggregate>().Id.Value));

        // Raw internal ID for backward compatibility
        descriptor
            .Field("internalId")
            .Type<NonNullType<UuidType>>()
            .Description("Internal UUID. Prefer using 'id' (global ID) for client operations.")
            .Resolve(ctx => ctx.Parent<UserProfileAggregate>().Id.Value);

        // User ID as global ID
        descriptor
            .Field("userId")
            .Type<NonNullType<IdType>>()
            .Description("Global ID of the user who owns this profile.")
            .Resolve(ctx => GlobalIdSerializer.Serialize("User", ctx.Parent<UserProfileAggregate>().UserId.Value));

        // Display name
        descriptor
            .Field(p => p.DisplayName)
            .Type<NonNullType<StringType>>()
            .Resolve(ctx => ctx.Parent<UserProfileAggregate>().DisplayName.Value);

        // Birthday (optional) - visible to family members
        descriptor
            .Field(p => p.Birthday)
            .Type<DateType>()
            .Directive(new VisibleDirective { To = FieldVisibility.Family })
            .Resolve(ctx => ctx.Parent<UserProfileAggregate>().Birthday?.Value);

        // Calculated age - visible to family members (derived from birthday)
        descriptor
            .Field("age")
            .Type<IntType>()
            .Description("Calculated age based on birthday.")
            .Directive(new VisibleDirective { To = FieldVisibility.Family })
            .Resolve(ctx =>
            {
                var birthday = ctx.Parent<UserProfileAggregate>().Birthday?.Value;
                if (birthday is null) return null;

                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var age = today.Year - birthday.Value.Year;
                if (birthday.Value > today.AddYears(-age)) age--;
                return age;
            });

        // Pronouns (optional) - visible to all authenticated users
        descriptor
            .Field(p => p.Pronouns)
            .Type<StringType>()
            .Directive(new VisibleDirective { To = FieldVisibility.Public })
            .Resolve(ctx => ctx.Parent<UserProfileAggregate>().Pronouns?.Value);

        // Preferences - visible only to profile owner
        descriptor
            .Field(p => p.Preferences)
            .Type<NonNullType<ProfilePreferencesObjectType>>()
            .Directive(new VisibleDirective { To = FieldVisibility.Owner });

        // Field visibility settings - visible only to profile owner
        descriptor
            .Field(p => p.FieldVisibility)
            .Type<NonNullType<ProfileFieldVisibilityObjectType>>()
            .Directive(new VisibleDirective { To = FieldVisibility.Owner });

        // Audit info
        descriptor
            .Field("auditInfo")
            .Type<NonNullType<ObjectType<AuditInfoType>>>()
            .Resolve(ctx =>
            {
                var profile = ctx.Parent<UserProfileAggregate>();
                return new AuditInfoType
                {
                    CreatedAt = profile.CreatedAt,
                    UpdatedAt = profile.UpdatedAt
                };
            });
    }
}

/// <summary>
/// ObjectType for ProfilePreferences value object.
/// </summary>
public sealed class ProfilePreferencesObjectType : ObjectType<Domain.ValueObjects.ProfilePreferences>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<Domain.ValueObjects.ProfilePreferences> descriptor)
    {
        descriptor.Name("ProfilePreferences");
        descriptor.Description("User's localization and display preferences.");

        descriptor
            .Field(p => p.Language)
            .Type<NonNullType<StringType>>()
            .Resolve(ctx => ctx.Parent<Domain.ValueObjects.ProfilePreferences>().Language);

        descriptor
            .Field(p => p.Timezone)
            .Type<NonNullType<StringType>>()
            .Resolve(ctx => ctx.Parent<Domain.ValueObjects.ProfilePreferences>().Timezone);

        descriptor
            .Field(p => p.DateFormat)
            .Type<NonNullType<StringType>>()
            .Resolve(ctx => ctx.Parent<Domain.ValueObjects.ProfilePreferences>().DateFormat);
    }
}

/// <summary>
/// ObjectType for ProfileFieldVisibility value object.
/// </summary>
public sealed class ProfileFieldVisibilityObjectType : ObjectType<Domain.ValueObjects.ProfileFieldVisibility>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<Domain.ValueObjects.ProfileFieldVisibility> descriptor)
    {
        descriptor.Name("ProfileFieldVisibility");
        descriptor.Description("Visibility settings for profile fields.");

        descriptor
            .Field(p => p.BirthdayVisibility)
            .Type<NonNullType<EnumType<Domain.ValueObjects.VisibilityLevel>>>();

        descriptor
            .Field(p => p.PronounsVisibility)
            .Type<NonNullType<EnumType<Domain.ValueObjects.VisibilityLevel>>>();

        descriptor
            .Field(p => p.PreferencesVisibility)
            .Type<NonNullType<EnumType<Domain.ValueObjects.VisibilityLevel>>>();
    }
}
