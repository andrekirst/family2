using System.Linq.Expressions;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.Specifications;
using UserProfileAggregate = FamilyHub.Modules.UserProfile.Domain.Aggregates.UserProfile;

namespace FamilyHub.Modules.UserProfile.Domain.Specifications;

/// <summary>
/// Specification for finding a user profile by its ID.
/// </summary>
/// <param name="profileId">The profile ID to search for.</param>
public sealed class ProfileByIdSpecification(UserProfileId profileId) : Specification<UserProfileAggregate>
{
    /// <inheritdoc/>
    public override Expression<Func<UserProfileAggregate, bool>> ToExpression()
        => profile => profile.Id == profileId;
}
