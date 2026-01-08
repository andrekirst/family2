using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Mappers;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

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
    /// Resolves all members belonging to a family.
    /// </summary>
    /// <param name="family">The parent family entity.</param>
    /// <param name="dbContext">Auth database context service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of users belonging to the family.</returns>
    [GraphQLDescription("All members belonging to this family")]
    public async Task<IReadOnlyList<User>> GetMembers(
        [Parent] FamilyAggregate family,
        [Service] AuthDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var members = await dbContext.Users
            .Where(u => u.FamilyId == family.Id)
            .ToListAsync(cancellationToken);

        return members.AsReadOnly();
    }

    /// <summary>
    /// Resolves the owner of the family.
    /// Replaces the scalar ownerId field with a nested UserType for richer owner information.
    /// </summary>
    /// <param name="family">The parent family entity.</param>
    /// <param name="userRepository">User repository service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The owner as UserType, or null if not found.</returns>
    [GraphQLDescription("The owner of this family")]
    public async Task<UserType?> GetOwner(
        [Parent] FamilyAggregate family,
        [Service] IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var owner = await userRepository.GetByIdAsync(family.OwnerId, cancellationToken);

        return owner == null ? null : UserMapper.AsGraphQLType(owner);
    }
}
