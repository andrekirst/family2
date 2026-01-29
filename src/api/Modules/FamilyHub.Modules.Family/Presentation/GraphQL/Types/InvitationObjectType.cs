using FamilyHub.Infrastructure.GraphQL.Types;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Presentation.GraphQL.Relay;
using HotChocolate;
using HotChocolate.Types;
using InvitationAggregate = FamilyHub.Modules.Family.Domain.Aggregates.FamilyMemberInvitation;

namespace FamilyHub.Modules.Family.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL ObjectType configuration for the FamilyMemberInvitation entity.
/// Implements the Relay Node interface for global ID resolution.
/// </summary>
/// <remarks>
/// <para>
/// This type:
/// <list type="bullet">
/// <item><description>Exposes FamilyMemberInvitation entity as a GraphQL "Invitation" type</description></item>
/// <item><description>Implements Node interface with base64-encoded global IDs</description></item>
/// <item><description>Provides node resolution via repository lookup</description></item>
/// </list>
/// </para>
/// <para>
/// The global ID format is: base64("Invitation:{guid}")
/// </para>
/// </remarks>
public sealed class InvitationObjectType : ObjectType<InvitationAggregate>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<InvitationAggregate> descriptor)
    {
        descriptor.Name("Invitation");
        descriptor.Description("A family membership invitation sent via email.");

        descriptor.BindFieldsExplicitly();

        // NOTE: Relay Node interface temporarily disabled due to HotChocolate IdField expression validation
        // issue with Vogen value objects. The pattern .IdField(i => i.Id.Value) is rejected as
        // "not a property-expression or method-call-expression". Fix tracked in issue #XXX.

        // Global ID field (Relay-compatible format)
        descriptor
            .Field("id")
            .Type<NonNullType<IdType>>()
            .Description("Global ID (Relay Node specification)")
            .Resolve(ctx => GlobalIdSerializer.Serialize("Invitation", ctx.Parent<InvitationAggregate>().Id.Value));

        // Raw internal ID for backward compatibility
        descriptor
            .Field("internalId")
            .Type<NonNullType<UuidType>>()
            .Description("Internal UUID. Prefer using 'id' (global ID) for client operations.")
            .Resolve(ctx => ctx.Parent<InvitationAggregate>().Id.Value);

        // Family ID as global ID
        descriptor
            .Field("familyId")
            .Type<NonNullType<IdType>>()
            .Description("Global ID of the family this invitation is for.")
            .Resolve(ctx => GlobalIdSerializer.Serialize("Family", ctx.Parent<InvitationAggregate>().FamilyId.Value));

        // Email address
        descriptor
            .Field(i => i.Email)
            .Type<NonNullType<StringType>>()
            .Description("Email address of the invited person.")
            .Resolve(ctx => ctx.Parent<InvitationAggregate>().Email.Value);

        // Role to be assigned - map Vogen FamilyRole to GraphQL FamilyRoleType enum
        descriptor
            .Field("role")
            .Type<NonNullType<EnumType<FamilyRoleType>>>()
            .Description("Role that will be assigned when the invitation is accepted.")
            .Resolve(ctx => MapFamilyRole(ctx.Parent<InvitationAggregate>().Role));

        // Status
        descriptor
            .Field(i => i.Status)
            .Type<NonNullType<InvitationStatusEnumType>>()
            .Description("Current status of the invitation.");

        // Display code (for debugging/support)
        descriptor
            .Field("displayCode")
            .Type<NonNullType<StringType>>()
            .Description("User-friendly display code for debugging and support.")
            .Resolve(ctx => ctx.Parent<InvitationAggregate>().DisplayCode.Value);

        // Expires at
        descriptor
            .Field(i => i.ExpiresAt)
            .Type<NonNullType<DateTimeType>>()
            .Description("When the invitation expires.");

        // Invited by user ID as global ID
        descriptor
            .Field("invitedByUserId")
            .Type<NonNullType<IdType>>()
            .Description("Global ID of the user who created the invitation.")
            .Resolve(ctx => GlobalIdSerializer.Serialize("User", ctx.Parent<InvitationAggregate>().InvitedByUserId.Value));

        // Optional message
        descriptor
            .Field(i => i.Message)
            .Type<StringType>()
            .Description("Optional personal message included with the invitation.");

        // Accepted at (null if not accepted)
        descriptor
            .Field(i => i.AcceptedAt)
            .Type<DateTimeType>()
            .Description("When the invitation was accepted (null if not yet accepted).");

        // Computed field: isExpired
        descriptor
            .Field("isExpired")
            .Type<NonNullType<BooleanType>>()
            .Description("Whether the invitation has expired.")
            .Resolve(ctx => ctx.Parent<InvitationAggregate>().ExpiresAt < DateTime.UtcNow);

        // Audit info
        descriptor
            .Field("auditInfo")
            .Type<NonNullType<ObjectType<AuditInfoType>>>()
            .Description("Audit metadata (creation and last update timestamps).")
            .Resolve(ctx =>
            {
                var invitation = ctx.Parent<InvitationAggregate>();
                return new AuditInfoType
                {
                    CreatedAt = invitation.CreatedAt,
                    UpdatedAt = invitation.UpdatedAt
                };
            });
    }

    private static FamilyRoleType MapFamilyRole(FamilyRole role) =>
        role.Value.ToLowerInvariant() switch
        {
            "owner" => FamilyRoleType.OWNER,
            "admin" => FamilyRoleType.ADMIN,
            "member" => FamilyRoleType.MEMBER,
            "child" => FamilyRoleType.CHILD,
            _ => throw new InvalidOperationException($"Unknown family role: {role.Value}")
        };
}

/// <summary>
/// GraphQL EnumType for InvitationStatus.
/// </summary>
public sealed class InvitationStatusEnumType : EnumType<InvitationStatus>
{
    /// <inheritdoc />
    protected override void Configure(IEnumTypeDescriptor<InvitationStatus> descriptor)
    {
        descriptor.Name("InvitationStatus");
        descriptor.Description("Status of a family invitation.");

        descriptor.BindValuesExplicitly();

        descriptor.Value(InvitationStatus.Pending)
            .Name("PENDING")
            .Description("Invitation is pending acceptance.");

        descriptor.Value(InvitationStatus.Accepted)
            .Name("ACCEPTED")
            .Description("Invitation has been accepted.");

        descriptor.Value(InvitationStatus.Expired)
            .Name("EXPIRED")
            .Description("Invitation has expired.");

        descriptor.Value(InvitationStatus.Canceled)
            .Name("CANCELED")
            .Description("Invitation has been canceled.");
    }
}
