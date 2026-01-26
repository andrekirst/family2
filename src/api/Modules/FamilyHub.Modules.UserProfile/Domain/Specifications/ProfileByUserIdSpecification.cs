using System.Linq.Expressions;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using UserProfileAggregate = FamilyHub.Modules.UserProfile.Domain.Aggregates.UserProfile;

namespace FamilyHub.Modules.UserProfile.Domain.Specifications;

/// <summary>
/// Specification for finding a user profile by user ID.
/// </summary>
/// <param name="userId">The user ID to search for.</param>
public sealed class ProfileByUserIdSpecification(UserId userId) : Specification<UserProfileAggregate>
{
    /// <inheritdoc/>
    public override Expression<Func<UserProfileAggregate, bool>> ToExpression()
        => profile => profile.UserId == userId;
}
