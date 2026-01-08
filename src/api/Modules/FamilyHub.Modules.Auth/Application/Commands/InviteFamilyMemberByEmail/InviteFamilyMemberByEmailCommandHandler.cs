using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.InviteFamilyMemberByEmail;

/// <summary>
/// Handler for InviteFamilyMemberByEmailCommand.
/// Validates business rules and creates an email-based invitation.
/// User context and authorization are handled by pipeline behaviors.
/// </summary>
public sealed partial class InviteFamilyMemberByEmailCommandHandler(
    IUserContext userContext,
    IFamilyRepository familyRepository,
    IFamilyMemberInvitationRepository invitationRepository,
    IUnitOfWork unitOfWork,
    ILogger<InviteFamilyMemberByEmailCommandHandler> logger)
    : IRequestHandler<InviteFamilyMemberByEmailCommand, FamilyHub.SharedKernel.Domain.Result<InviteFamilyMemberByEmailResult>>
{
    public async Task<FamilyHub.SharedKernel.Domain.Result<InviteFamilyMemberByEmailResult>> Handle(
        InviteFamilyMemberByEmailCommand request,
        CancellationToken cancellationToken)
    {
        // Get user context (already loaded and validated by behaviors)
        var currentUserId = userContext.UserId;
        LogInvitingMemberToFamily(request.Email.Value, request.FamilyId.Value, currentUserId.Value);

        // 1. Validate family exists
        var family = await familyRepository.GetByIdAsync(request.FamilyId, cancellationToken);
        if (family == null)
        {
            LogFamilyNotFound(request.FamilyId.Value);
            return Result.Failure<InviteFamilyMemberByEmailResult>("Family not found.");
        }

        // 2. Check if email is already a family member
        var isExistingMember = await invitationRepository.IsUserMemberOfFamilyAsync(request.FamilyId, request.Email, cancellationToken);
        if (isExistingMember)
        {
            LogEmailAlreadyMember(request.Email.Value);
            return Result.Failure<InviteFamilyMemberByEmailResult>($"Email '{request.Email.Value}' is already a member of this family.");
        }

        // 3. Check for duplicate pending invitation
        var existingInvitation = await invitationRepository.GetPendingByEmailAsync(request.FamilyId, request.Email, cancellationToken);
        if (existingInvitation != null)
        {
            LogDuplicateInvitation(request.Email.Value);
            return Result.Failure<InviteFamilyMemberByEmailResult>($"Email '{request.Email.Value}' already has a pending invitation.");
        }

        // 4. Validate role (cannot invite as OWNER)
        if (request.Role == FamilyRole.Owner)
        {
            LogInvalidRole("OWNER");
            return Result.Failure<InviteFamilyMemberByEmailResult>("Cannot invite a member as OWNER. Each family can have only one owner.");
        }

        // 5. Create invitation using domain factory method
        var invitation = Family.Domain.FamilyMemberInvitation.CreateEmailInvitation(
            familyId: request.FamilyId,
            email: request.Email,
            role: request.Role,
            invitedByUserId: currentUserId,
            message: request.Message
        );

        // 6. Persist to database
        await invitationRepository.AddAsync(invitation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogInvitationCreated(invitation.Id.Value, request.Email.Value);

        // 7. Return result
        return Result.Success(new InviteFamilyMemberByEmailResult
        {
            InvitationId = invitation.Id,
            Email = invitation.Email,
            Role = invitation.Role,
            Token = invitation.Token,
            DisplayCode = invitation.DisplayCode,
            ExpiresAt = invitation.ExpiresAt,
            Status = invitation.Status
        });
    }

    [LoggerMessage(LogLevel.Information, "Inviting member '{email}' to family {familyId} by user {userId}")]
    partial void LogInvitingMemberToFamily(string email, Guid familyId, Guid userId);

    [LoggerMessage(LogLevel.Warning, "Family {familyId} not found")]
    partial void LogFamilyNotFound(Guid familyId);

    [LoggerMessage(LogLevel.Warning, "Email '{email}' is already a member of the family")]
    partial void LogEmailAlreadyMember(string email);

    [LoggerMessage(LogLevel.Warning, "Email '{email}' already has a pending invitation")]
    partial void LogDuplicateInvitation(string email);

    [LoggerMessage(LogLevel.Warning, "Invalid role: {role}")]
    partial void LogInvalidRole(string role);

    [LoggerMessage(LogLevel.Information, "Created invitation {invitationId} for email '{email}'")]
    partial void LogInvitationCreated(Guid invitationId, string email);
}
