using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile.Application.Queries.GetMyPendingChanges;

/// <summary>
/// Handler for GetMyPendingChangesQuery.
/// Returns the current user's pending and recently rejected change requests.
/// </summary>
/// <param name="userContext">The current authenticated user context.</param>
/// <param name="changeRequestRepository">Repository for profile change requests.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class GetMyPendingChangesQueryHandler(
    IUserContext userContext,
    IProfileChangeRequestRepository changeRequestRepository,
    ILogger<GetMyPendingChangesQueryHandler> logger)
    : IQueryHandler<GetMyPendingChangesQuery, GetMyPendingChangesResult>
{
    /// <inheritdoc />
    public async Task<GetMyPendingChangesResult> Handle(
        GetMyPendingChangesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        LogGettingMyPendingChanges(userId.Value);

        // Get pending change requests for the current user
        var allRequests = await changeRequestRepository.GetPendingByUserAsync(userId, cancellationToken);

        var pendingItems = new List<MyPendingChangeItem>();
        var rejectedItems = new List<MyRejectedChangeItem>();

        // Separate pending and recently rejected requests
        // Note: GetPendingByUserAsync returns pending ones, but we also want to show recent rejections
        // For now, we'll just show pending ones. To show rejections, we'd need a new repository method.
        foreach (var changeRequest in allRequests)
        {
            if (changeRequest.Status == ChangeRequestStatus.Pending)
            {
                pendingItems.Add(new MyPendingChangeItem
                {
                    RequestId = changeRequest.Id,
                    FieldName = changeRequest.FieldName,
                    OldValue = changeRequest.OldValue,
                    NewValue = changeRequest.NewValue,
                    CreatedAt = changeRequest.CreatedAt
                });
            }
        }

        // TODO: Add repository method to get recently rejected requests and populate rejectedItems
        // For now, return an empty list for recently rejected

        LogFoundMyPendingChanges(pendingItems.Count, userId.Value);

        return new GetMyPendingChangesResult
        {
            PendingChanges = pendingItems,
            RecentlyRejected = rejectedItems
        };
    }

    [LoggerMessage(LogLevel.Debug, "Getting my pending profile changes for user {userId}")]
    partial void LogGettingMyPendingChanges(Guid userId);

    [LoggerMessage(LogLevel.Debug, "Found {count} pending profile changes for user {userId}")]
    partial void LogFoundMyPendingChanges(int count, Guid userId);
}
