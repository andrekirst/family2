using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Family.Presentation.GraphQL.DataLoaders;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using HotChocolate.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// HotChocolate type extensions for UserType.
/// Adds resolver-based fields to the User GraphQL type.
/// </summary>
[ExtendObjectType(typeof(UserType))]
public sealed class UserTypeExtensions
{
    /// <summary>
    /// Resolves the family that a user belongs to using DataLoader for batching.
    /// NOTE: UserType DTO no longer includes FamilyId, so we must fetch the domain User entity first.
    /// The family lookup uses FamilyBatchDataLoader to batch multiple family lookups into a single query.
    /// </summary>
    /// <param name="user">The parent user DTO.</param>
    /// <param name="userRepository">User repository service (required to fetch domain entity for FamilyId).</param>
    /// <param name="familyDataLoader">Family batch data loader for N+1 prevention.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's family, or null if not found.</returns>
    [GraphQLDescription("The family this user belongs to")]
    public async Task<FamilyAggregate?> GetFamily(
        [Parent] UserType user,
        [Service] IUserRepository userRepository,
        FamilyBatchDataLoader familyDataLoader,
        CancellationToken cancellationToken)
    {
        // Fetch domain User entity to get FamilyId (not available in UserType DTO anymore)
        // NOTE: This still has N+1 potential for fetching domain users.
        // Future improvement: Add FamilyId to UserType DTO to eliminate this lookup.
        var domainUser = await userRepository.GetByIdAsync(
            UserId.From(user.Id),
            cancellationToken);

        if (domainUser == null)
        {
            return null;
        }

        // Family lookup now batched using DataLoader
        return await familyDataLoader.LoadAsync(domainUser.FamilyId, cancellationToken);
    }
}
