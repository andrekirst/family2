using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.SharedKernel.Application.Abstractions;

/// <summary>
/// Cross-module service interface for calendar operations.
/// Implemented by Calendar module (stub in UserProfile for now), consumed by UserProfile module.
///
/// PURPOSE: This interface enables event chain automation by providing a contract
/// for creating calendar events from profile changes (e.g., birthday → recurring event).
///
/// USAGE:
/// - UserProfile event handlers inject this interface for cross-module calendar operations
/// - Calendar module will provide the implementation when created (Phase 2)
/// - Currently uses stub implementation in UserProfile module (log-only)
///
/// ARCHITECTURE:
/// - Lives in SharedKernel (neutral ground) to avoid circular dependencies
/// - Calendar module implements (owns Calendar data)
/// - UserProfile module consumes (needs to create birthday events)
/// </summary>
public interface ICalendarService
{
    /// <summary>
    /// Creates a recurring birthday event in the family calendar.
    /// Called by BirthdaySetEventHandler when a user's birthday is set.
    /// </summary>
    /// <param name="familyId">The family ID to create the event for.</param>
    /// <param name="userId">The user whose birthday is being celebrated.</param>
    /// <param name="displayName">Display name for the event title (e.g., "John's Birthday").</param>
    /// <param name="birthday">The birthday date (used for recurring schedule).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateRecurringBirthdayEventAsync(
        FamilyId familyId,
        UserId userId,
        string displayName,
        DateOnly birthday,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing birthday event when the user's name changes.
    /// Called by DisplayNameChangedEventHandler to keep calendar event titles in sync.
    /// </summary>
    /// <param name="familyId">The family ID containing the event.</param>
    /// <param name="userId">The user whose event needs updating.</param>
    /// <param name="newDisplayName">The new display name for the event title.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateBirthdayEventTitleAsync(
        FamilyId familyId,
        UserId userId,
        string newDisplayName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a birthday event when the user's birthday is cleared.
    /// </summary>
    /// <param name="familyId">The family ID containing the event.</param>
    /// <param name="userId">The user whose event should be removed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveBirthdayEventAsync(
        FamilyId familyId,
        UserId userId,
        CancellationToken cancellationToken = default);
}
