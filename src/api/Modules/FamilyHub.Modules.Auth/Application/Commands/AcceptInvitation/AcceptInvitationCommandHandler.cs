using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.DTOs.Subscriptions;
using FamilyHub.Modules.Auth.Application.Services;
using FamilyHub.Modules.Family.Application.Abstractions;
using FamilyHub.Modules.Family.Domain.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation;

/// <summary>
/// Handler for AcceptInvitationCommand.
/// Accepts the invitation and updates user's family membership.
/// Validation is handled by AcceptInvitationCommandValidator.
/// User context is automatically provided by UserContextEnrichmentBehavior.
/// Entities are retrieved from IValidationCache (populated by validator).
/// SPECIAL CASE: User may not have a family yet (joining via invitation).
/// </summary>
/// <remarks>
/// This handler performs cross-module operations affecting both Auth (User) and Family (Invitation)
/// contexts. Both IUnitOfWork (Auth) and IFamilyUnitOfWork (Family) must be saved to persist all changes.
/// </remarks>
/// <param name="userContext">The current authenticated user context.</param>
/// <param name="invitationRepository">Repository for invitation data access.</param>
/// <param name="validationCache">Cache containing validated entities from validator.</param>
/// <param name="unitOfWork">Unit of work for Auth database transactions.</param>
/// <param name="familyUnitOfWork">Unit of work for Family database transactions.</param>
/// <param name="subscriptionPublisher">Publisher for real-time subscription events.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class AcceptInvitationCommandHandler(
    IUserContext userContext,
    IFamilyMemberInvitationRepository invitationRepository,
    IValidationCache validationCache,
    IUnitOfWork unitOfWork,
    IFamilyUnitOfWork familyUnitOfWork,
    FamilyHub.Modules.Auth.Application.Services.SubscriptionEventPublisher subscriptionPublisher,
    ILogger<AcceptInvitationCommandHandler> logger)
    : ICommandHandler<AcceptInvitationCommand, FamilyHub.SharedKernel.Domain.Result<AcceptInvitationResult>>
{
    /// <inheritdoc />
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

        // 4. Retrieve family DTO from cache (validator already fetched and validated via IFamilyService)
        var family = validationCache.Get<FamilyDto>(
            CacheKeyBuilder.Family(invitation.FamilyId.Value))
            ?? throw new InvalidOperationException("Family not found in cache. Validator should have cached it.");

        // 5. Update user's family and role
        currentUser.UpdateFamily(invitation.FamilyId);
        currentUser.UpdateRole(invitation.Role);

        // 6. Update invitation status
        await invitationRepository.UpdateAsync(invitation, cancellationToken);

        // 7. Commit changes to both contexts
        // - AuthDbContext: User family/role updates (tracked by UserContextEnrichmentBehavior)
        // - FamilyDbContext: Invitation status update
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await familyUnitOfWork.SaveChangesAsync(cancellationToken);

        LogInvitationAccepted(currentUserId.Value, family.Id.Value, invitation.Role.Value);

        // 8. Publish subscription events for real-time UI updates
        // Publish family member ADDED event
        await subscriptionPublisher.PublishFamilyMemberAddedAsync(
            invitation.FamilyId,
            new FamilyMemberDto
            {
                Id = currentUser.Id.Value,
                Email = currentUser.Email.Value,
                EmailVerified = currentUser.EmailVerified,
                Role = invitation.Role.Value,
                JoinedAt = DateTime.UtcNow,
                IsOwner = invitation.Role == FamilyRole.Owner,
                CreatedAt = currentUser.CreatedAt,
                UpdatedAt = currentUser.UpdatedAt
            },
            cancellationToken
        );

        // Publish invitation REMOVED event (invitation no longer pending)
        await subscriptionPublisher.PublishInvitationRemovedAsync(
            invitation.FamilyId,
            invitation.Token.Value,
            cancellationToken
        );

        // 9. Return result
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
