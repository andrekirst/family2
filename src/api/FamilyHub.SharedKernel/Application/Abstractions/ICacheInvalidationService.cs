using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.SharedKernel.Application.Abstractions;

/// <summary>
/// Cross-module service interface for cache invalidation operations.
/// Implemented by modules that own cached data, consumed by modules triggering changes.
///
/// PURPOSE: This interface enables cache coherency across modules by providing
/// a contract for invalidating cached data when source data changes.
///
/// USAGE:
/// - Event handlers inject this interface to invalidate caches after state changes
/// - Modules that cache cross-module data implement relevant methods
/// - Supports both specific key invalidation and pattern-based invalidation
///
/// ARCHITECTURE:
/// - Lives in SharedKernel (neutral ground) to avoid circular dependencies
/// - Multiple modules may implement (each for their own caches)
/// - Multiple modules may consume (to trigger invalidation)
/// </summary>
public interface ICacheInvalidationService
{
    /// <summary>
    /// Invalidates the family members cache for a specific family.
    /// Called when a family member's display name changes to ensure fresh data on next query.
    /// </summary>
    /// <param name="familyId">The family ID whose members cache should be invalidated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InvalidateFamilyMembersCacheAsync(
        FamilyId familyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates the user profile cache for a specific user.
    /// Called when profile data changes to ensure fresh data on next query.
    /// </summary>
    /// <param name="userId">The user ID whose profile cache should be invalidated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InvalidateUserProfileCacheAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all caches related to a specific user.
    /// Called during significant user changes (deletion, deactivation).
    /// </summary>
    /// <param name="userId">The user ID whose caches should be invalidated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InvalidateAllUserCachesAsync(
        UserId userId,
        CancellationToken cancellationToken = default);
}
