using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using DomainResult = FamilyHub.SharedKernel.Domain.Result;

namespace FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation;

/// <summary>
/// Handler for AcceptInvitationCommand.
/// Validates the invitation token, checks email match, and accepts the invitation.
/// </summary>
public sealed partial class AcceptInvitationCommandHandler(
    IFamilyMemberInvitationRepository invitationRepository,
    IUserRepository userRepository,
    IFamilyRepository familyRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ILogger<AcceptInvitationCommandHandler> logger)
    : IRequestHandler<AcceptInvitationCommand, FamilyHub.SharedKernel.Domain.Result<AcceptInvitationResult>>
{
    public async Task<FamilyHub.SharedKernel.Domain.Result<AcceptInvitationResult>> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        LogAcceptingInvitation(request.Token.Value);

        // 1. Fetch invitation
        var invitation = await invitationRepository.GetByTokenAsync(request.Token, cancellationToken);
        if (invitation == null)
        {
            LogInvitationNotFound(request.Token.Value);
            return DomainResult.Failure<AcceptInvitationResult>("Invalid or expired invitation token.");
        }

        // 2. Get current user
        var currentUserId = await currentUserService.GetUserIdAsync(cancellationToken);
        var currentUser = await userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (currentUser == null)
        {
            LogUserNotFound(currentUserId.Value);
            return DomainResult.Failure<AcceptInvitationResult>("Current user not found.");
        }

        // 3. Validate user email matches invitation email
        if (invitation.Email != currentUser.Email)
        {
            LogEmailMismatch(currentUser.Email.Value, invitation.Email.Value);
            return DomainResult.Failure<AcceptInvitationResult>("Invitation email does not match authenticated user.");
        }

        // 4. Accept invitation (domain method handles expiration check)
        try
        {
            invitation.Accept(currentUserId);
        }
        catch (InvalidOperationException ex)
        {
            LogAcceptFailed(ex.Message);
            return DomainResult.Failure<AcceptInvitationResult>(ex.Message);
        }

        // 5. Fetch family to get family name
        var family = await familyRepository.GetByIdAsync(invitation.FamilyId, cancellationToken);
        if (family == null)
        {
            LogFamilyNotFound(invitation.FamilyId.Value);
            return DomainResult.Failure<AcceptInvitationResult>("Family not found.");
        }

        // 6. Update user's family and role
        currentUser.UpdateFamily(invitation.FamilyId);
        currentUser.UpdateRole(invitation.Role);

        // 7. Update invitation status
        await invitationRepository.UpdateAsync(invitation, cancellationToken);

        // 8. Update user
        userRepository.Update(currentUser);

        // 9. Commit changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogInvitationAccepted(currentUserId.Value, family.Id.Value, invitation.Role.Value);

        // 10. Return result
        return Result.Success(new AcceptInvitationResult
        {
            FamilyId = invitation.FamilyId,
            FamilyName = family.Name,
            Role = invitation.Role
        });
    }

    [LoggerMessage(LogLevel.Information, "Accepting invitation with token {token}")]
    partial void LogAcceptingInvitation(string token);

    [LoggerMessage(LogLevel.Warning, "Invitation not found for token {token}")]
    partial void LogInvitationNotFound(string token);

    [LoggerMessage(LogLevel.Warning, "User {userId} not found")]
    partial void LogUserNotFound(Guid userId);

    [LoggerMessage(LogLevel.Warning, "Email mismatch: user email {userEmail} does not match invitation email {invitationEmail}")]
    partial void LogEmailMismatch(string userEmail, string invitationEmail);

    [LoggerMessage(LogLevel.Warning, "Failed to accept invitation: {message}")]
    partial void LogAcceptFailed(string message);

    [LoggerMessage(LogLevel.Warning, "Family {familyId} not found")]
    partial void LogFamilyNotFound(Guid familyId);

    [LoggerMessage(LogLevel.Information, "User {userId} accepted invitation and joined family {familyId} with role {role}")]
    partial void LogInvitationAccepted(Guid userId, Guid familyId, string role);
}
