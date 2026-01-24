using FamilyHub.Modules.Family.Domain.Abstractions;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.Specifications;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Family.Application.Commands.InviteFamilyMembers;

/// <summary>
/// Handler for InviteFamilyMembersCommand.
/// Processes batch invitations with partial success support.
/// Valid invitations succeed, invalid ones return errors without failing the entire batch.
/// User context and authorization are handled by pipeline behaviors.
/// </summary>
/// <param name="userContext">The current authenticated user context.</param>
/// <param name="familyRepository">Repository for family data access.</param>
/// <param name="invitationRepository">Repository for invitation data access.</param>
/// <param name="userLookupService">Service for cross-module user lookups.</param>
/// <param name="familyUnitOfWork">Unit of work for Family module database transactions.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class InviteFamilyMembersCommandHandler(
    IUserContext userContext,
    IFamilyRepository familyRepository,
    IFamilyMemberInvitationRepository invitationRepository,
    IUserLookupService userLookupService,
    IFamilyUnitOfWork familyUnitOfWork,
    ILogger<InviteFamilyMembersCommandHandler> logger)
    : ICommandHandler<InviteFamilyMembersCommand, InviteFamilyMembersResult>
{
    /// <summary>
    /// Handles the InviteFamilyMembersCommand to invite multiple users to a family.
    /// Processes each invitation individually to support partial success.
    /// </summary>
    /// <param name="request">The command containing the batch invitation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing successful and failed invitations.</returns>
    public async Task<InviteFamilyMembersResult> Handle(
        InviteFamilyMembersCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = userContext.UserId;
        var currentUserEmail = userContext.Email;
        LogBatchInvitationStarted(request.Invitations.Count, request.FamilyId.Value, currentUserId.Value);

        // 1. Validate family exists (fail fast for all invitations)
        var family = await familyRepository.GetByIdAsync(request.FamilyId, cancellationToken);
        if (family == null)
        {
            LogFamilyNotFound(request.FamilyId.Value);
            // Return all invitations as failed since family doesn't exist
            return CreateAllFailedResult(request.Invitations, InvitationErrorCode.UNKNOWN, "Family not found.");
        }

        var successfulInvitations = new List<InvitationSuccess>();
        var failedInvitations = new List<InvitationFailure>();

        // 2. Check for duplicate emails within the batch
        var duplicateEmails = request.Invitations
            .GroupBy(i => i.Email)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();

        // 3. Process each invitation
        foreach (var invitationRequest in request.Invitations)
        {
            var validationResult = await ValidateInvitationAsync(
                invitationRequest,
                request.FamilyId,
                currentUserEmail,
                duplicateEmails,
                cancellationToken);

            if (validationResult.IsValid)
            {
                // Create and persist invitation
                var invitation = FamilyMemberInvitation.CreateEmailInvitation(
                    familyId: request.FamilyId,
                    email: invitationRequest.Email,
                    role: invitationRequest.Role,
                    invitedByUserId: currentUserId,
                    message: request.Message
                );

                await invitationRepository.AddAsync(invitation, cancellationToken);

                successfulInvitations.Add(new InvitationSuccess
                {
                    InvitationId = invitation.Id,
                    Email = invitation.Email,
                    Role = invitation.Role,
                    Token = invitation.Token,
                    DisplayCode = invitation.DisplayCode,
                    ExpiresAt = invitation.ExpiresAt,
                    Status = invitation.Status
                });

                LogInvitationCreated(invitation.Id.Value, invitationRequest.Email.Value);
            }
            else
            {
                failedInvitations.Add(new InvitationFailure
                {
                    Email = invitationRequest.Email,
                    Role = invitationRequest.Role,
                    ErrorCode = validationResult.ErrorCode,
                    ErrorMessage = validationResult.ErrorMessage
                });

                LogInvitationFailed(invitationRequest.Email.Value, validationResult.ErrorCode.ToString(), validationResult.ErrorMessage);
            }
        }

        // 4. Save all successful invitations in one transaction
        if (successfulInvitations.Count > 0)
        {
            await familyUnitOfWork.SaveChangesAsync(cancellationToken);
        }

        LogBatchInvitationCompleted(successfulInvitations.Count, failedInvitations.Count);

        return new InviteFamilyMembersResult
        {
            SuccessfulInvitations = successfulInvitations,
            FailedInvitations = failedInvitations
        };
    }

    private async Task<InvitationValidationResult> ValidateInvitationAsync(
        InvitationRequest invitation,
        FamilyId familyId,
        Email currentUserEmail,
        HashSet<Email> duplicateEmailsInBatch,
        CancellationToken cancellationToken)
    {
        // 1. Check for self-invite
        if (invitation.Email == currentUserEmail)
        {
            return InvitationValidationResult.Failure(
                InvitationErrorCode.SELF_INVITE,
                "Cannot invite yourself.");
        }

        // 2. Check for duplicate in batch
        if (duplicateEmailsInBatch.Contains(invitation.Email))
        {
            return InvitationValidationResult.Failure(
                InvitationErrorCode.DUPLICATE_IN_BATCH,
                "Duplicate email in batch request.");
        }

        // 3. Validate role (cannot invite as OWNER)
        if (invitation.Role == FamilyRole.Owner)
        {
            return InvitationValidationResult.Failure(
                InvitationErrorCode.INVALID_ROLE,
                "Cannot invite a member as OWNER. Each family can have only one owner.");
        }

        // 4. Check if email is already a member of THIS family
        var isExistingMember = await userLookupService.IsEmailMemberOfFamilyAsync(familyId, invitation.Email, cancellationToken);
        if (isExistingMember)
        {
            return InvitationValidationResult.Failure(
                InvitationErrorCode.ALREADY_MEMBER,
                $"Email '{invitation.Email.Value}' is already a member of this family.");
        }

        // 5. Check if email is already a member of ANOTHER family (cross-family check)
        var existingFamilyId = await userLookupService.GetFamilyIdByEmailAsync(invitation.Email, cancellationToken);
        if (existingFamilyId != null && existingFamilyId != familyId)
        {
            return InvitationValidationResult.Failure(
                InvitationErrorCode.MEMBER_OF_ANOTHER_FAMILY,
                $"Email '{invitation.Email.Value}' is already a member of another family.");
        }

        // 6. Check for duplicate pending invitation
        var existingInvitation = await invitationRepository.FindOneAsync(
            new PendingInvitationByEmailSpecification(familyId, invitation.Email),
            cancellationToken);
        if (existingInvitation != null)
        {
            return InvitationValidationResult.Failure(
                InvitationErrorCode.DUPLICATE_PENDING_INVITATION,
                $"Email '{invitation.Email.Value}' already has a pending invitation.");
        }

        return InvitationValidationResult.Success();
    }

    private static InviteFamilyMembersResult CreateAllFailedResult(
        IReadOnlyList<InvitationRequest> invitations,
        InvitationErrorCode errorCode,
        string errorMessage)
    {
        return new InviteFamilyMembersResult
        {
            SuccessfulInvitations = [],
            FailedInvitations = invitations.Select(i => new InvitationFailure
            {
                Email = i.Email,
                Role = i.Role,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            }).ToList()
        };
    }

    // Logging methods using LoggerMessage source generation
    [LoggerMessage(LogLevel.Information, "Starting batch invitation of {count} members to family {familyId} by user {userId}")]
    partial void LogBatchInvitationStarted(int count, Guid familyId, Guid userId);

    [LoggerMessage(LogLevel.Warning, "Family {familyId} not found")]
    partial void LogFamilyNotFound(Guid familyId);

    [LoggerMessage(LogLevel.Information, "Created invitation {invitationId} for email '{email}'")]
    partial void LogInvitationCreated(Guid invitationId, string email);

    [LoggerMessage(LogLevel.Warning, "Invitation failed for email '{email}': {errorCode} - {errorMessage}")]
    partial void LogInvitationFailed(string email, string errorCode, string errorMessage);

    [LoggerMessage(LogLevel.Information, "Batch invitation completed: {successCount} successful, {failureCount} failed")]
    partial void LogBatchInvitationCompleted(int successCount, int failureCount);
}

/// <summary>
/// Internal result type for individual invitation validation.
/// </summary>
internal readonly struct InvitationValidationResult
{
    public bool IsValid { get; }
    public InvitationErrorCode ErrorCode { get; }
    public string ErrorMessage { get; }

    private InvitationValidationResult(bool isValid, InvitationErrorCode errorCode, string errorMessage)
    {
        IsValid = isValid;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static InvitationValidationResult Success() => new(true, default, string.Empty);
    public static InvitationValidationResult Failure(InvitationErrorCode errorCode, string errorMessage) => new(false, errorCode, errorMessage);
}
