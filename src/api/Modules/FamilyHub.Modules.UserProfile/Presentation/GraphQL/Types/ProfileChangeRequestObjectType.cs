using FamilyHub.Infrastructure.GraphQL.Types;
using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Presentation.GraphQL.Relay;
using HotChocolate;
using HotChocolate.Types;
using ChangeRequestAggregate = FamilyHub.Modules.UserProfile.Domain.Aggregates.ProfileChangeRequest;

namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL ObjectType configuration for the ProfileChangeRequest entity.
/// Implements the Relay Node interface for global ID resolution.
/// </summary>
/// <remarks>
/// <para>
/// This type:
/// <list type="bullet">
/// <item><description>Exposes ProfileChangeRequest entity as a GraphQL "ProfileChangeRequest" type</description></item>
/// <item><description>Implements Node interface with base64-encoded global IDs</description></item>
/// <item><description>Provides node resolution via repository lookup</description></item>
/// </list>
/// </para>
/// <para>
/// The global ID format is: base64("ProfileChangeRequest:{guid}")
/// </para>
/// <para>
/// ProfileChangeRequests are created when child users attempt to modify their profiles.
/// Parents/admins can then approve or reject these changes.
/// </para>
/// </remarks>
public sealed class ProfileChangeRequestObjectType : ObjectType<ChangeRequestAggregate>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<ChangeRequestAggregate> descriptor)
    {
        descriptor.Name("ProfileChangeRequest");
        descriptor.Description("A pending profile change that requires approval from a parent or admin.");

        descriptor.BindFieldsExplicitly();

        // Implement Relay Node interface
        descriptor
            .ImplementsNode()
            .IdField(r => r.Id.Value)
            .ResolveNode(async (ctx, id) =>
            {
                var repository = ctx.Service<IProfileChangeRequestRepository>();
                return await repository.GetByIdAsync(ChangeRequestId.From(id), ctx.RequestAborted);
            });

        // Override the ID field to return global ID
        descriptor
            .Field("id")
            .Type<NonNullType<IdType>>()
            .Description("Global ID (Relay Node specification)")
            .Resolve(ctx => GlobalIdSerializer.Serialize("ProfileChangeRequest", ctx.Parent<ChangeRequestAggregate>().Id.Value));

        // Raw internal ID for backward compatibility
        descriptor
            .Field("internalId")
            .Type<NonNullType<UuidType>>()
            .Description("Internal UUID. Prefer using 'id' (global ID) for client operations.")
            .Resolve(ctx => ctx.Parent<ChangeRequestAggregate>().Id.Value);

        // Profile ID as global ID
        descriptor
            .Field("profileId")
            .Type<NonNullType<IdType>>()
            .Description("Global ID of the user profile this change request applies to.")
            .Resolve(ctx => GlobalIdSerializer.Serialize("UserProfile", ctx.Parent<ChangeRequestAggregate>().ProfileId.Value));

        // Requested by user ID as global ID
        descriptor
            .Field("requestedBy")
            .Type<NonNullType<IdType>>()
            .Description("Global ID of the user who requested the change.")
            .Resolve(ctx => GlobalIdSerializer.Serialize("User", ctx.Parent<ChangeRequestAggregate>().RequestedBy.Value));

        // Family ID as global ID
        descriptor
            .Field("familyId")
            .Type<NonNullType<IdType>>()
            .Description("Global ID of the family for routing to approvers.")
            .Resolve(ctx => GlobalIdSerializer.Serialize("Family", ctx.Parent<ChangeRequestAggregate>().FamilyId.Value));

        // Field name
        descriptor
            .Field(r => r.FieldName)
            .Type<NonNullType<StringType>>()
            .Description("The name of the profile field being changed (e.g., 'DisplayName', 'Birthday').");

        // Old value
        descriptor
            .Field(r => r.OldValue)
            .Type<StringType>()
            .Description("The current value of the field (null if not previously set).");

        // New value
        descriptor
            .Field(r => r.NewValue)
            .Type<NonNullType<StringType>>()
            .Description("The requested new value for the field.");

        // Status - convert from Vogen string value object to GraphQL enum
        descriptor
            .Field("status")
            .Type<NonNullType<ChangeRequestStatusEnumType>>()
            .Description("Current status of the change request.")
            .Resolve(ctx =>
            {
                var status = ctx.Parent<ChangeRequestAggregate>().Status.Value.ToUpperInvariant();
                return status;
            });

        // Reviewed by user ID as global ID (optional)
        descriptor
            .Field("reviewedBy")
            .Type<IdType>()
            .Description("Global ID of the user who reviewed the request (null if still pending).")
            .Resolve(ctx =>
            {
                var reviewedBy = ctx.Parent<ChangeRequestAggregate>().ReviewedBy;
                return reviewedBy.HasValue
                    ? GlobalIdSerializer.Serialize("User", reviewedBy.Value.Value)
                    : null;
            });

        // Reviewed at (optional)
        descriptor
            .Field(r => r.ReviewedAt)
            .Type<DateTimeType>()
            .Description("When the request was reviewed (null if still pending).");

        // Rejection reason (optional)
        descriptor
            .Field(r => r.RejectionReason)
            .Type<StringType>()
            .Description("Reason provided for rejection (null if approved or pending).");

        // Computed fields for convenience
        descriptor
            .Field(r => r.IsPending)
            .Type<NonNullType<BooleanType>>()
            .Description("Whether this request is still pending approval.");

        descriptor
            .Field(r => r.IsApproved)
            .Type<NonNullType<BooleanType>>()
            .Description("Whether this request has been approved.");

        descriptor
            .Field(r => r.IsRejected)
            .Type<NonNullType<BooleanType>>()
            .Description("Whether this request has been rejected.");

        // Audit info
        descriptor
            .Field("auditInfo")
            .Type<NonNullType<ObjectType<AuditInfoType>>>()
            .Description("Audit metadata (creation and last update timestamps).")
            .Resolve(ctx =>
            {
                var request = ctx.Parent<ChangeRequestAggregate>();
                return new AuditInfoType
                {
                    CreatedAt = request.CreatedAt,
                    UpdatedAt = request.UpdatedAt
                };
            });
    }
}

/// <summary>
/// GraphQL EnumType for ChangeRequestStatus.
/// </summary>
/// <remarks>
/// Maps the Vogen string-based ChangeRequestStatus to a GraphQL enum for type safety.
/// </remarks>
public sealed class ChangeRequestStatusEnumType : EnumType
{
    /// <inheritdoc />
    protected override void Configure(IEnumTypeDescriptor descriptor)
    {
        descriptor.Name("ChangeRequestStatus");
        descriptor.Description("Status of a profile change request.");

        descriptor.Value("PENDING")
            .Description("Change request is pending approval.");

        descriptor.Value("APPROVED")
            .Description("Change request has been approved and applied.");

        descriptor.Value("REJECTED")
            .Description("Change request has been rejected.");
    }
}
