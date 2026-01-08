using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// HotChocolate type extensions for UserType.
/// Adds resolver-based fields to the User GraphQL type.
/// </summary>
[ExtendObjectType(typeof(UserType))]
public sealed class UserTypeExtensions
{
    /// <summary>
    /// Resolves the family that a user belongs to.
    /// NOTE: UserType DTO no longer includes FamilyId, so we must fetch the domain User entity first.
    /// </summary>
    /// <param name="user">The parent user DTO.</param>
    /// <param name="userRepository">User repository service (required to fetch domain entity).</param>
    /// <param name="familyRepository">Family repository service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's family, or null if not found.</returns>
    [GraphQLDescription("The family this user belongs to")]
    public async Task<FamilyHub.Modules.Family.Domain.Family?> GetFamily(
        [Parent] UserType user,
        [Service] IUserRepository userRepository,
        [Service] IFamilyRepository familyRepository,
        CancellationToken cancellationToken)
    {
        // Fetch domain User entity to get FamilyId (not available in UserType DTO anymore)
        var domainUser = await userRepository.GetByIdAsync(
            UserId.From(user.Id),
            cancellationToken);

        if (domainUser == null)
            return null;

        // Now fetch the family using the domain user's FamilyId
        return await familyRepository.GetByIdAsync(domainUser.FamilyId, cancellationToken);
    }
}
