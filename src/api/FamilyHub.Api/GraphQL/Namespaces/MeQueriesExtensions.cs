using FamilyHub.Api.Application.Services;
using FamilyHub.Api.GraphQL.Types;
using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;

namespace FamilyHub.Api.GraphQL.Namespaces;

/// <summary>
/// Extends MeQueries with user-centric query fields.
/// </summary>
/// <remarks>
/// <para>
/// Access pattern:
/// <code>
/// query {
///   me {
///     profile { displayName, auditInfo { ... } }
///     family {
///       ... on Family { name, members { ... } }
///       ... on NotCreatedReason { reason, message }
///       ... on InvitePendingReason { pendingCount }
///       ... on LeftFamilyReason { leftAt }
///     }
///     pendingInvitations { email, familyName }
///   }
/// }
/// </code>
/// </para>
/// </remarks>
[ExtendObjectType(typeof(MeQueries))]
public sealed class MeQueriesExtensions
{
    /// <summary>
    /// Gets the current user's profile.
    /// </summary>
    /// <param name="coordinator">The cross-module query coordinator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's profile, or null if not found.</returns>
    [Authorize]
    [GraphQLDescription("Get the current user's profile with full details.")]
    public async Task<UserProfileDto?> Profile(
        [Service] IMeQueryCoordinator coordinator,
        CancellationToken cancellationToken)
    {
        return await coordinator.GetMyProfileAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the current user's family or a reason why they have no family.
    /// </summary>
    /// <param name="coordinator">The cross-module query coordinator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A union type that is either:
    /// - Family: The user's family with members and invitations
    /// - NotCreatedReason: User hasn't created a family
    /// - InvitePendingReason: User has pending invitations
    /// - LeftFamilyReason: User left their previous family
    /// </returns>
    [Authorize]
    [GraphQLDescription("Get the current user's family or reason for not having one.")]
    [GraphQLType(typeof(FamilyOrReasonUnionType))]
    public async Task<object> Family(
        [Service] IMeQueryCoordinator coordinator,
        CancellationToken cancellationToken)
    {
        var result = await coordinator.GetFamilyOrReasonAsync(cancellationToken);
        return result.ToGraphQlType();
    }

    /// <summary>
    /// Gets invitations received by the current user (not yet accepted).
    /// </summary>
    /// <param name="coordinator">The cross-module query coordinator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of pending invitations received by the user.</returns>
    /// <remarks>
    /// This is distinct from family.invitations which shows invitations
    /// SENT BY the user's family. This field shows invitations RECEIVED
    /// by the user (when they don't have a family yet or are being invited
    /// to another family).
    /// </remarks>
    [Authorize]
    [GraphQLDescription("Get invitations you have received (for joining families).")]
    public async Task<List<ReceivedInvitationDto>> PendingInvitations(
        [Service] IMeQueryCoordinator coordinator,
        CancellationToken cancellationToken)
    {
        // TODO: Implement when GetReceivedInvitationsQuery is created
        // For now, return empty list as the query doesn't exist yet
        var count = await coordinator.GetPendingInvitationCountAsync(cancellationToken);
        return new List<ReceivedInvitationDto>();
    }
}

/// <summary>
/// DTO for invitations received by the user.
/// </summary>
public sealed record ReceivedInvitationDto
{
    /// <summary>
    /// The invitation ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Name of the family that sent the invitation.
    /// </summary>
    public required string FamilyName { get; init; }

    /// <summary>
    /// Role that will be assigned when accepted.
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Name of the person who sent the invitation.
    /// </summary>
    public required string InvitedByName { get; init; }

    /// <summary>
    /// Optional personal message from the inviter.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// When the invitation was sent.
    /// </summary>
    public required DateTime InvitedAt { get; init; }

    /// <summary>
    /// When the invitation expires.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Whether the invitation has expired.
    /// </summary>
    public bool IsExpired => ExpiresAt < DateTime.UtcNow;
}
