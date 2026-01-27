using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.SharedKernel.Application.Abstractions;

/// <summary>
/// Cross-module service interface for user lookup operations.
/// Implemented by Auth module, consumed by Family module.
///
/// PURPOSE: This interface enables proper bounded context separation by providing
/// a defined contract for cross-module queries without direct entity references.
///
/// USAGE:
/// - Family module repositories inject this interface for cross-module queries
/// - Auth module provides the implementation using AuthDbContext
/// - Returns only value objects/primitives (no entity leakage)
///
/// ARCHITECTURE:
/// - Lives in SharedKernel (neutral ground) to avoid circular dependencies
/// - Auth module implements (owns User data)
/// - Family module consumes (needs User lookups for invitation validation)
/// </summary>
public interface IUserLookupService
{
    /// <summary>
    /// Gets the FamilyId for a given user.
    /// Used by FamilyRepository.GetFamilyByUserIdAsync().
    /// </summary>
    /// <param name="userId">The user ID to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's FamilyId if they belong to a family; otherwise, null.</returns>
    Task<FamilyId?> GetUserFamilyIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts users belonging to a family.
    /// Used by FamilyRepository.GetMemberCountAsync().
    /// </summary>
    /// <param name="familyId">The family ID to count members for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of users in the family.</returns>
    Task<int> GetFamilyMemberCountAsync(FamilyId familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user with the given email is a member of the family.
    /// Used by FamilyMemberInvitationRepository.IsUserMemberOfFamilyAsync().
    /// </summary>
    /// <param name="familyId">The family ID to check membership in.</param>
    /// <param name="email">The email address to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a user with this email is in the family; otherwise, false.</returns>
    Task<bool> IsEmailMemberOfFamilyAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the FamilyId that a user with the given email belongs to, if any.
    /// Used for cross-family validation (preventing users from being invited to multiple families).
    /// </summary>
    /// <param name="email">The email address to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The FamilyId if user exists and has a family; otherwise, null.</returns>
    Task<FamilyId?> GetFamilyIdByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the email address of a user by their user ID.
    /// Used by Family module event handlers for email generation.
    /// </summary>
    /// <param name="userId">The user ID to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's email address if found; otherwise, null.</returns>
    Task<Email?> GetUserEmailAsync(UserId userId, CancellationToken cancellationToken = default);
}
