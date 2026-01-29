using FamilyHub.Modules.Family.Presentation.GraphQL.Types;
using HotChocolate.Types;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Api.GraphQL.Types;

/// <summary>
/// Union type for the family query result.
/// Returns either a Family or a reason why the user has no family.
/// </summary>
/// <remarks>
/// <para>
/// This union type enables rich error states in the GraphQL schema:
/// <code>
/// query {
///   me {
///     family {
///       ... on Family { name members { ... } }
///       ... on NotCreatedReason { reason message }
///       ... on InvitePendingReason { reason pendingCount }
///       ... on LeftFamilyReason { reason leftAt }
///     }
///   }
/// }
/// </code>
/// </para>
/// <para>
/// The frontend can use these types to show appropriate onboarding flows
/// based on the user's current family status.
/// </para>
/// </remarks>
public sealed class FamilyOrReasonUnionType : UnionType
{
    /// <inheritdoc />
    protected override void Configure(IUnionTypeDescriptor descriptor)
    {
        descriptor.Name("FamilyOrReason");
        descriptor.Description(
            "Either a Family or a reason why the user has no family. " +
            "Use GraphQL fragments to access type-specific fields.");

        // Include Family type
        descriptor.Type<FamilyType>();

        // Include all NoFamilyReason types
        descriptor.Type<NotCreatedReasonType>();
        descriptor.Type<InvitePendingReasonType>();
        descriptor.Type<LeftFamilyReasonType>();
    }
}

/// <summary>
/// Result wrapper for the family query that can hold either a Family or a reason.
/// </summary>
/// <remarks>
/// This is used by the MeQueryCoordinator to return the appropriate type
/// based on the user's family status.
/// </remarks>
public abstract record FamilyOrReasonResult
{
    /// <summary>
    /// Creates a successful result with a Family.
    /// </summary>
    public static FamilyOrReasonResult Success(FamilyAggregate family) => new FamilyResult(family);

    /// <summary>
    /// Creates a result indicating the user hasn't created a family.
    /// </summary>
    public static FamilyOrReasonResult NotCreated() => new NotCreatedResult();

    /// <summary>
    /// Creates a result indicating the user has pending invitations.
    /// </summary>
    public static FamilyOrReasonResult InvitePending(int pendingCount) => new InvitePendingResult(pendingCount);

    /// <summary>
    /// Creates a result indicating the user left their family.
    /// </summary>
    public static FamilyOrReasonResult LeftFamily(DateTime leftAt) => new LeftFamilyResult(leftAt);

    /// <summary>
    /// Converts this result to the appropriate GraphQL type.
    /// </summary>
    public abstract object ToGraphQlType();

    private sealed record FamilyResult(FamilyAggregate Family) : FamilyOrReasonResult
    {
        public override object ToGraphQlType() => Family;
    }

    private sealed record NotCreatedResult : FamilyOrReasonResult
    {
        public override object ToGraphQlType() => new NotCreatedReason();
    }

    private sealed record InvitePendingResult(int PendingCount) : FamilyOrReasonResult
    {
        public override object ToGraphQlType() => new InvitePendingReason { PendingCount = PendingCount };
    }

    private sealed record LeftFamilyResult(DateTime LeftAt) : FamilyOrReasonResult
    {
        public override object ToGraphQlType() => new LeftFamilyReason { LeftAt = LeftAt };
    }
}
