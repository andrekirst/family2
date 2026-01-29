using FamilyHub.Infrastructure.GraphQL.Types;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Mappers;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Presentation.GraphQL.Relay;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Relay;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL ObjectType configuration for the User entity.
/// Implements the Relay Node interface for global ID resolution.
/// </summary>
/// <remarks>
/// <para>
/// This type:
/// <list type="bullet">
/// <item><description>Exposes User entity as a GraphQL "User" type</description></item>
/// <item><description>Implements Node interface with base64-encoded global IDs</description></item>
/// <item><description>Provides node resolution via repository lookup</description></item>
/// </list>
/// </para>
/// <para>
/// The global ID format is: base64("User:{guid}")
/// </para>
/// </remarks>
public sealed class UserObjectType : ObjectType<User>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.Name("User");
        descriptor.Description("A registered user in the Family Hub system.");

        // NOTE: Relay Node interface temporarily disabled due to HotChocolate IdField expression validation
        // issue with Vogen value objects. The pattern .IdField(u => u.Id.Value) is rejected as
        // "not a property-expression or method-call-expression". Fix tracked in issue #XXX.
        // Users can still query by ID using regular query fields.

        // Global ID field (Relay-compatible format)
        descriptor
            .Field("id")
            .Type<NonNullType<IdType>>()
            .Resolve(ctx =>
            {
                var user = ctx.Parent<User>();
                return GlobalIdSerializer.Serialize("User", user.Id.Value);
            });

        // Raw internal ID for backward compatibility (optional)
        descriptor
            .Field("internalId")
            .Type<NonNullType<UuidType>>()
            .Description("Internal UUID. Prefer using 'id' (global ID) for client operations.")
            .Resolve(ctx => ctx.Parent<User>().Id.Value);

        // Email field
        descriptor
            .Field(u => u.Email)
            .Type<NonNullType<StringType>>()
            .Resolve(ctx => ctx.Parent<User>().Email.Value);

        // Email verified field
        descriptor
            .Field(u => u.EmailVerified)
            .Type<NonNullType<BooleanType>>();

        // Family ID (as global ID for the family)
        descriptor
            .Field("familyId")
            .Type<NonNullType<IdType>>()
            .Description("The ID of the family this user belongs to.")
            .Resolve(ctx => GlobalIdSerializer.Serialize("Family", ctx.Parent<User>().FamilyId.Value));

        // User's role in the family - map Vogen FamilyRole to GraphQL UserRoleType enum
        descriptor
            .Field("role")
            .Type<NonNullType<EnumType<UserRoleType>>>()
            .Resolve(ctx => ctx.Parent<User>().Role.AsRoleType());

        // Audit info
        descriptor
            .Field("auditInfo")
            .Type<NonNullType<ObjectType<AuditInfoType>>>()
            .Resolve(ctx =>
            {
                var user = ctx.Parent<User>();
                return new AuditInfoType
                {
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };
            });

        // Ignore internal/sensitive fields
        descriptor.Ignore(u => u.PasswordHash);
        descriptor.Ignore(u => u.FailedLoginAttempts);
        descriptor.Ignore(u => u.LockoutEndTime);
        descriptor.Ignore(u => u.IsLockedOut);
        descriptor.Ignore(u => u.EmailVerificationToken);
        descriptor.Ignore(u => u.EmailVerificationTokenExpiresAt);
        descriptor.Ignore(u => u.PasswordResetToken);
        descriptor.Ignore(u => u.PasswordResetTokenExpiresAt);
        descriptor.Ignore(u => u.PasswordResetCode);
        descriptor.Ignore(u => u.PasswordResetCodeExpiresAt);
        descriptor.Ignore(u => u.DeletedAt);
        descriptor.Ignore(u => u.ExternalLogins);
        descriptor.Ignore(u => u.RefreshTokens);
        descriptor.Ignore(u => u.EmailVerifiedAt);
    }
}
