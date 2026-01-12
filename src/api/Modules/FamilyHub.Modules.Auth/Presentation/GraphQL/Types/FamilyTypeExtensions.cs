using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Presentation.GraphQL.DataLoaders;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Mappers;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Presentation.GraphQL.DataLoaders;
using HotChocolate.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// HotChocolate type extensions for Family entity.
/// Adds resolver-based fields to the Family GraphQL type.
///
/// PHASE 4: Extends Family.Domain.Aggregates.Family (moved to Family module).
/// Remains in Auth module temporarily because it accesses User aggregate and AuthDbContext.
/// TODO Phase 5+: Move to Family module when proper bounded context separation is implemented.
/// </summary>
[ExtendObjectType(typeof(FamilyAggregate))]
public sealed class FamilyTypeExtensions
{
    /// <summary>
    /// Resolves all members belonging to a family using GroupedDataLoader for batching.
    /// Batches multiple family member lookups into a single WHERE family_id IN (...) query.
    /// </summary>
    /// <param name="family">The parent family entity.</param>
    /// <param name="dataLoader">Users by family grouped data loader for N+1 prevention.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of users belonging to the family.</returns>
    [GraphQLDescription("All members belonging to this family")]
    public async Task<IEnumerable<User>> GetMembers(
        [Parent] FamilyAggregate family,
        UsersByFamilyGroupedDataLoader dataLoader,
        CancellationToken cancellationToken)
    {
        var members = await dataLoader.LoadAsync(family.Id, cancellationToken);
        return members ?? [];
    }

    /// <summary>
    /// Resolves the owner of the family using DataLoader for batching.
    /// Replaces the scalar ownerId field with a nested UserType for richer owner information.
    /// Uses UserBatchDataLoader to batch multiple owner lookups into a single query.
    /// </summary>
    /// <param name="family">The parent family entity.</param>
    /// <param name="userDataLoader">User batch data loader for N+1 prevention.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The owner as UserType, or null if not found.</returns>
    [GraphQLDescription("The owner of this family")]
    public async Task<UserType?> GetOwner(
        [Parent] FamilyAggregate family,
        UserBatchDataLoader userDataLoader,
        CancellationToken cancellationToken)
    {
        var owner = await userDataLoader.LoadAsync(family.OwnerId, cancellationToken);

        return owner == null ? null : UserMapper.AsGraphQLType(owner);
    }

    /// <summary>
    /// Resolves all invitations for a family using GroupedDataLoader for batching.
    /// Batches multiple family invitation lookups into a single WHERE family_id IN (...) query.
    /// </summary>
    /// <param name="family">The parent family entity.</param>
    /// <param name="dataLoader">Invitations by family grouped data loader for N+1 prevention.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of invitations for the family.</returns>
    [GraphQLDescription("All invitations for this family")]
    public async Task<IEnumerable<PendingInvitationType>> GetInvitations(
        [Parent] FamilyAggregate family,
        InvitationsByFamilyGroupedDataLoader dataLoader,
        CancellationToken cancellationToken)
    {
        var invitations = await dataLoader.LoadAsync(family.Id, cancellationToken);
        return invitations?.Select(MapToPendingInvitationType) ?? [];
    }

    /// <summary>
    /// Maps FamilyMemberInvitation domain entity to PendingInvitationType GraphQL type.
    /// </summary>
    private static PendingInvitationType MapToPendingInvitationType(FamilyMemberInvitation invitation)
    {
        return new PendingInvitationType
        {
            Id = invitation.Id.Value,
            Email = invitation.Email.Value,
            Role = invitation.Role.AsRoleType(),
            Status = invitation.Status.AsStatusType(),
            InvitedById = invitation.InvitedByUserId.Value,
            // InvitedAt is calculated from ExpiresAt - 14 days (default invitation duration)
            InvitedAt = invitation.ExpiresAt.AddDays(-14),
            ExpiresAt = invitation.ExpiresAt,
            Message = invitation.Message,
            DisplayCode = invitation.DisplayCode.Value
        };
    }
}
