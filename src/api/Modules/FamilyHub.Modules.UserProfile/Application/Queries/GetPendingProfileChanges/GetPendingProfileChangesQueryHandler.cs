using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile.Application.Queries.GetPendingProfileChanges;

/// <summary>
/// Handler for GetPendingProfileChangesQuery.
/// Returns all pending profile change requests for the current user's family.
/// </summary>
/// <param name="userContext">The current authenticated user context.</param>
/// <param name="changeRequestRepository">Repository for profile change requests.</param>
/// <param name="profileRepository">Repository for user profiles (to get display names).</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class GetPendingProfileChangesQueryHandler(
    IUserContext userContext,
    IProfileChangeRequestRepository changeRequestRepository,
    IUserProfileRepository profileRepository,
    ILogger<GetPendingProfileChangesQueryHandler> logger)
    : IQueryHandler<GetPendingProfileChangesQuery, GetPendingProfileChangesResult>
{
    /// <inheritdoc />
    public async Task<GetPendingProfileChangesResult> Handle(
        GetPendingProfileChangesQuery request,
        CancellationToken cancellationToken)
    {
        var familyId = userContext.FamilyId;
        LogGettingPendingChanges(familyId.Value);

        // Get all pending change requests for the family
        var pendingRequests = await changeRequestRepository.GetPendingByFamilyAsync(familyId, cancellationToken);

        // Build the result items, fetching display names for each requester
        var items = new List<PendingChangeRequestItem>();
        var displayNameCache = new Dictionary<Guid, string?>();

        foreach (var changeRequest in pendingRequests)
        {
            // Get display name from cache or fetch it
            if (!displayNameCache.TryGetValue(changeRequest.RequestedBy.Value, out var displayName))
            {
                var profile = await profileRepository.GetByUserIdAsync(changeRequest.RequestedBy, cancellationToken);
                displayName = profile?.DisplayName.Value;
                displayNameCache[changeRequest.RequestedBy.Value] = displayName;
            }

            items.Add(new PendingChangeRequestItem
            {
                RequestId = changeRequest.Id,
                ProfileId = changeRequest.ProfileId,
                RequestedBy = changeRequest.RequestedBy,
                RequestedByDisplayName = displayName,
                FieldName = changeRequest.FieldName,
                OldValue = changeRequest.OldValue,
                NewValue = changeRequest.NewValue,
                CreatedAt = changeRequest.CreatedAt
            });
        }

        LogFoundPendingChanges(items.Count, familyId.Value);

        return new GetPendingProfileChangesResult
        {
            ChangeRequests = items
        };
    }

    [LoggerMessage(LogLevel.Debug, "Getting pending profile changes for family {familyId}")]
    partial void LogGettingPendingChanges(Guid familyId);

    [LoggerMessage(LogLevel.Debug, "Found {count} pending profile changes for family {familyId}")]
    partial void LogFoundPendingChanges(int count, Guid familyId);
}
