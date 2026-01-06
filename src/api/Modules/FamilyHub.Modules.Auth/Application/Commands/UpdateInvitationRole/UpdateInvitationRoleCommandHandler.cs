using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.UpdateInvitationRole;

/// <summary>
/// Handler for UpdateInvitationRoleCommand.
/// Validates authorization and updates the role of a pending invitation.
/// </summary>
public sealed partial class UpdateInvitationRoleCommandHandler(
    IFamilyMemberInvitationRepository invitationRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ILogger<UpdateInvitationRoleCommandHandler> logger)
    : IRequestHandler<UpdateInvitationRoleCommand, FamilyHub.SharedKernel.Domain.Result<UpdateInvitationRoleResult>>
{
    public async Task<FamilyHub.SharedKernel.Domain.Result<UpdateInvitationRoleResult>> Handle(
        UpdateInvitationRoleCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get authenticated user
        var currentUserId = await currentUserService.GetUserIdAsync(cancellationToken);
        LogUpdatingInvitationRole(request.InvitationId.Value, request.NewRole.Value, currentUserId.Value);

        // 2. Get invitation
        var invitation = await invitationRepository.GetByIdAsync(request.InvitationId, cancellationToken);
        if (invitation == null)
        {
            LogInvitationNotFound(request.InvitationId.Value);
            return SharedKernel.Domain.Result.Failure<UpdateInvitationRoleResult>("Invitation not found.");
        }

        // 3. Validate user is owner or admin
        var currentUser = await userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (currentUser == null)
        {
            LogUserNotFound(currentUserId.Value);
            return SharedKernel.Domain.Result.Failure<UpdateInvitationRoleResult>("Current user not found.");
        }

        if (currentUser.Role != UserRole.Owner && currentUser.Role != UserRole.Admin)
        {
            LogUnauthorized(currentUserId.Value, currentUser.Role.Value);
            return SharedKernel.Domain.Result.Failure<UpdateInvitationRoleResult>("Only OWNER or ADMIN can update invitation roles.");
        }

        // 4. Validate new role (cannot update to OWNER)
        if (request.NewRole == UserRole.Owner)
        {
            LogInvalidRole("OWNER");
            return SharedKernel.Domain.Result.Failure<UpdateInvitationRoleResult>("Cannot update invitation role to OWNER.");
        }

        // 5. Validate invitation is pending
        if (invitation.Status != InvitationStatus.Pending)
        {
            LogInvalidStatus(invitation.Status.Value);
            return SharedKernel.Domain.Result.Failure<UpdateInvitationRoleResult>("Can only update pending invitations.");
        }

        // 6. Update role using domain method
        try
        {
            invitation.UpdateRole(request.NewRole);
        }
        catch (InvalidOperationException ex)
        {
            LogDomainError(ex.Message);
            return SharedKernel.Domain.Result.Failure<UpdateInvitationRoleResult>(ex.Message);
        }

        // 7. Persist
        await invitationRepository.UpdateAsync(invitation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogRoleUpdated(request.InvitationId.Value, request.NewRole.Value);

        // 8. Return result
        return SharedKernel.Domain.Result.Success(new UpdateInvitationRoleResult
        {
            InvitationId = invitation.Id,
            Role = invitation.Role
        });
    }

    [LoggerMessage(LogLevel.Information, "Updating invitation {invitationId} role to {newRole} by user {userId}")]
    partial void LogUpdatingInvitationRole(Guid invitationId, string newRole, Guid userId);

    [LoggerMessage(LogLevel.Warning, "Invitation {invitationId} not found")]
    partial void LogInvitationNotFound(Guid invitationId);

    [LoggerMessage(LogLevel.Warning, "User {userId} not found")]
    partial void LogUserNotFound(Guid userId);

    [LoggerMessage(LogLevel.Warning, "User {userId} with role {role} is not authorized to update invitation roles")]
    partial void LogUnauthorized(Guid userId, string role);

    [LoggerMessage(LogLevel.Warning, "Invalid role: {role}")]
    partial void LogInvalidRole(string role);

    [LoggerMessage(LogLevel.Warning, "Cannot update invitation in status: {status}")]
    partial void LogInvalidStatus(string status);

    [LoggerMessage(LogLevel.Warning, "Domain error: {message}")]
    partial void LogDomainError(string message);

    [LoggerMessage(LogLevel.Information, "Updated invitation {invitationId} role to {newRole}")]
    partial void LogRoleUpdated(Guid invitationId, string newRole);
}
