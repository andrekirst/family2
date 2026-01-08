using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Services;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using DomainResult = FamilyHub.SharedKernel.Domain.Result;

namespace FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation;

/// <summary>
/// Handler for AcceptInvitationCommand.
/// Accepts the invitation and updates user's family membership.
/// Validation is handled by AcceptInvitationCommandValidator.
/// User context is automatically provided by UserContextEnrichmentBehavior.
/// Entities are retrieved from IValidationCache (populated by validator).
/// SPECIAL CASE: User may not have a family yet (joining via invitation).
/// </summary>
public sealed partial class AcceptInvitationCommandHandler(
    IUserContext userContext,
    IFamilyMemberInvitationRepository invitationRepository,
    IValidationCache validationCache,
    IUnitOfWork unitOfWork,
    ILogger<AcceptInvitationCommandHandler> logger)
    : IRequestHandler<AcceptInvitationCommand, FamilyHub.SharedKernel.Domain.Result<AcceptInvitationResult>>
{
    public async Task<FamilyHub.SharedKernel.Domain.Result<AcceptInvitationResult>> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        LogAcceptingInvitation(request.Token.Value);

        // Validation is handled by AcceptInvitationCommandValidator
        // 1. Retrieve invitation from cache (validator already fetched and validated)
        var invitation = validationCache.Get<FamilyMemberInvitationAggregate>(
            CacheKeyBuilder.FamilyMemberInvitation(request.Token.Value))
            ?? throw new InvalidOperationException("Invitation not found in cache. Validator should have cached it.");

        // 2. Get current user (loaded by UserContextEnrichmentBehavior)
        var currentUserId = userContext.UserId;
        var currentUser = userContext.User;

        // 3. Accept invitation (validator checked all prerequisites)
        invitation.Accept(currentUserId);

        // 4. Retrieve family from cache (validator already fetched and validated)
        var family = validationCache.Get<FamilyAggregate>(
            CacheKeyBuilder.Family(invitation.FamilyId.Value))
            ?? throw new InvalidOperationException("Family not found in cache. Validator should have cached it.");

        // 5. Update user's family and role
        currentUser.UpdateFamily(invitation.FamilyId);
        currentUser.UpdateRole(invitation.Role);

        // 6. Update invitation status
        await invitationRepository.UpdateAsync(invitation, cancellationToken);

        // 7. Commit changes (User already tracked by EF Core from UserContextEnrichmentBehavior)
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogInvitationAccepted(currentUserId.Value, family.Id.Value, invitation.Role.Value);

        // 8. Return result
        return Result.Success(new AcceptInvitationResult
        {
            FamilyId = invitation.FamilyId,
            FamilyName = family.Name,
            Role = invitation.Role
        });
    }

    [LoggerMessage(LogLevel.Information, "Accepting invitation with token {token}")]
    partial void LogAcceptingInvitation(string token);

    [LoggerMessage(LogLevel.Information, "User {userId} accepted invitation and joined family {familyId} with role {role}")]
    partial void LogInvitationAccepted(Guid userId, Guid familyId, string role);
}
