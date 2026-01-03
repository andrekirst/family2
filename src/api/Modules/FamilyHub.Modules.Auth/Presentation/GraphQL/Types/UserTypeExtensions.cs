using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
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
    /// Resolves the family that a user belongs to.
    /// </summary>
    /// <param name="user">The parent user DTO.</param>
    /// <param name="familyRepository">Family repository service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's family, or null if not found.</returns>
    [GraphQLDescription("The family this user belongs to")]
    public async Task<Family?> GetFamily(
        [Parent] UserType user,
        [Service] IFamilyRepository familyRepository,
        CancellationToken cancellationToken)
    {
        var familyId = FamilyId.From(user.FamilyId);
        return await familyRepository.GetByIdAsync(familyId, cancellationToken);
    }
}
