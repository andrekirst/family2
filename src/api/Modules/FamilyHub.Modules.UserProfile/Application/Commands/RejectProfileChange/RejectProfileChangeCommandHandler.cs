using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Persistence;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using DomainResult = FamilyHub.SharedKernel.Domain.Result<FamilyHub.Modules.UserProfile.Application.Commands.RejectProfileChange.RejectProfileChangeResult>;

namespace FamilyHub.Modules.UserProfile.Application.Commands.RejectProfileChange;

/// <summary>
/// Handler for RejectProfileChangeCommand.
/// Marks the change request as rejected with the provided reason.
/// </summary>
public sealed partial class RejectProfileChangeCommandHandler(
    IUserContext userContext,
    IProfileChangeRequestRepository changeRequestRepository,
    UserProfileDbContext dbContext,
    ILogger<RejectProfileChangeCommandHandler> logger)
    : ICommandHandler<RejectProfileChangeCommand, DomainResult>
{
    /// <inheritdoc />
    public async Task<DomainResult> Handle(
        RejectProfileChangeCommand request,
        CancellationToken cancellationToken)
    {
        var rejecterId = userContext.UserId;
        LogRejectingChangeRequest(request.RequestId.Value, rejecterId.Value);

        // Load the change request
        var changeRequest = await changeRequestRepository.GetByIdAsync(request.RequestId, cancellationToken);

        if (changeRequest == null)
        {
            LogChangeRequestNotFound(request.RequestId.Value);
            return Result.Failure<RejectProfileChangeResult>("Change request not found.");
        }

        // Verify the request is still pending
        if (!changeRequest.IsPending)
        {
            LogChangeRequestNotPending(request.RequestId.Value, changeRequest.Status.Value);
            return Result.Failure<RejectProfileChangeResult>(
                $"Change request is not pending. Current status: {changeRequest.Status.Value}");
        }

        // Mark the request as rejected (this raises ProfileChangeRejectedEvent)
        changeRequest.Reject(rejecterId, request.Reason);

        // Persist atomically
        await dbContext.SaveChangesAsync(cancellationToken);

        LogChangeRequestRejected(request.RequestId.Value, changeRequest.FieldName, request.Reason);

        return DomainResult.Success(new RejectProfileChangeResult
        {
            RequestId = changeRequest.Id,
            ProfileId = changeRequest.ProfileId,
            FieldName = changeRequest.FieldName,
            Reason = request.Reason,
            RejectedAt = changeRequest.ReviewedAt!.Value
        });
    }

    [LoggerMessage(LogLevel.Information, "Rejecting change request {requestId} by user {rejecterId}")]
    partial void LogRejectingChangeRequest(Guid requestId, Guid rejecterId);

    [LoggerMessage(LogLevel.Warning, "Change request {requestId} not found")]
    partial void LogChangeRequestNotFound(Guid requestId);

    [LoggerMessage(LogLevel.Warning, "Change request {requestId} is not pending, status: {status}")]
    partial void LogChangeRequestNotPending(Guid requestId, string status);

    [LoggerMessage(LogLevel.Information, "Change request {requestId} rejected for field {fieldName}. Reason: {reason}")]
    partial void LogChangeRequestRejected(Guid requestId, string fieldName, string reason);
}
