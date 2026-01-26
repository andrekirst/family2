using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Interfaces;
using UserProfileAggregate = FamilyHub.Modules.UserProfile.Domain.Aggregates.UserProfile;

namespace FamilyHub.Modules.UserProfile.Domain.Repositories;

/// <summary>
/// Repository interface for UserProfile aggregate root.
/// Follows DDD repository pattern - operates on aggregate roots only.
/// Extends ISpecificationRepository to support specification-based queries.
/// </summary>
public interface IUserProfileRepository : ISpecificationRepository<UserProfileAggregate, UserProfileId>
{
    /// <summary>
    /// Gets the user profile for a specific user.
    /// This is a convenience method for the common use case.
    /// </summary>
    /// <param name="userId">The user ID to find the profile for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user profile if found; otherwise, null.</returns>
    Task<UserProfileAggregate?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
}
