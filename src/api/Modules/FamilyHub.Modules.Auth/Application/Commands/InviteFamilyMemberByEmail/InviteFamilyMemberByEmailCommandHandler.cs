using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.InviteFamilyMemberByEmail;

/// <summary>
/// Handler for InviteFamilyMemberByEmailCommand.
/// Validates business rules and creates an email-based invitation.
/// </summary>
public sealed partial class InviteFamilyMemberByEmailCommandHandler(
    IFamilyRepository familyRepository,
    IUserRepository userRepository,
    IFamilyMemberInvitationRepository invitationRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ILogger<InviteFamilyMemberByEmailCommandHandler> logger)
    : IRequestHandler<InviteFamilyMemberByEmailCommand, FamilyHub.SharedKernel.Domain.Result<InviteFamilyMemberByEmailResult>>
{
    public async Task<FamilyHub.SharedKernel.Domain.Result<InviteFamilyMemberByEmailResult>> Handle(
        InviteFamilyMemberByEmailCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get authenticated user
        var currentUserId = await currentUserService.GetUserIdAsync(cancellationToken);
        LogInvitingMemberToFamily(request.Email.Value, request.FamilyId.Value, currentUserId.Value);

        // 2. Validate family exists
        var family = await familyRepository.GetByIdAsync(request.FamilyId, cancellationToken);
        if (family == null)
        {
            LogFamilyNotFound(request.FamilyId.Value);
            return Result.Failure<InviteFamilyMemberByEmailResult>("Family not found.");
        }

        // 3. Validate user is owner or admin (authorization check)
        var currentUser = await userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (currentUser == null)
        {
            LogUserNotFound(currentUserId.Value);
            return Result.Failure<InviteFamilyMemberByEmailResult>("Current user not found.");
        }

        if (currentUser.Role != UserRole.Owner && currentUser.Role != UserRole.Admin)
        {
            LogUnauthorized(currentUserId.Value, currentUser.Role.Value);
            return Result.Failure<InviteFamilyMemberByEmailResult>("Only OWNER or ADMIN can invite family members.");
        }

        // 4. Check if email is already a family member
        var isExistingMember = await invitationRepository.IsUserMemberOfFamilyAsync(request.FamilyId, request.Email, cancellationToken);
        if (isExistingMember)
        {
            LogEmailAlreadyMember(request.Email.Value);
            return Result.Failure<InviteFamilyMemberByEmailResult>($"Email '{request.Email.Value}' is already a member of this family.");
        }

        // 5. Check for duplicate pending invitation
        var existingInvitation = await invitationRepository.GetPendingByEmailAsync(request.FamilyId, request.Email, cancellationToken);
        if (existingInvitation != null)
        {
            LogDuplicateInvitation(request.Email.Value);
            return Result.Failure<InviteFamilyMemberByEmailResult>($"Email '{request.Email.Value}' already has a pending invitation.");
        }

        // 6. Validate role (cannot invite as OWNER)
        if (request.Role == UserRole.Owner)
        {
            LogInvalidRole("OWNER");
            return Result.Failure<InviteFamilyMemberByEmailResult>("Cannot invite a member as OWNER. Each family can have only one owner.");
        }

        // 7. Create invitation using domain factory method
        var invitation = FamilyMemberInvitation.CreateEmailInvitation(
            familyId: request.FamilyId,
            email: request.Email,
            role: request.Role,
            invitedByUserId: currentUserId,
            message: request.Message
        );

        // 8. Persist to database
        await invitationRepository.AddAsync(invitation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogInvitationCreated(invitation.Id.Value, request.Email.Value);

        // 9. Return result
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

    [LoggerMessage(LogLevel.Warning, "User {userId} not found")]
    partial void LogUserNotFound(Guid userId);

    [LoggerMessage(LogLevel.Warning, "User {userId} with role {role} is not authorized to invite members")]
    partial void LogUnauthorized(Guid userId, string role);

    [LoggerMessage(LogLevel.Warning, "Email '{email}' is already a member of the family")]
    partial void LogEmailAlreadyMember(string email);

    [LoggerMessage(LogLevel.Warning, "Email '{email}' already has a pending invitation")]
    partial void LogDuplicateInvitation(string email);

    [LoggerMessage(LogLevel.Warning, "Invalid role: {role}")]
    partial void LogInvalidRole(string role);

    [LoggerMessage(LogLevel.Information, "Created invitation {invitationId} for email '{email}'")]
    partial void LogInvitationCreated(Guid invitationId, string email);
}
