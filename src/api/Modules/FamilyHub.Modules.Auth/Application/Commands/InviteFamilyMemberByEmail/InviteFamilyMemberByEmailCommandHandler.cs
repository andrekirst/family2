using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Family.Application.Abstractions;
using FamilyHub.Modules.Family.Domain.Specifications;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;
using IUserLookupService = FamilyHub.SharedKernel.Application.Abstractions.IUserLookupService;

namespace FamilyHub.Modules.Auth.Application.Commands.InviteFamilyMemberByEmail;

/// <summary>
/// Handler for InviteFamilyMemberByEmailCommand.
/// Validates business rules and creates an email-based invitation.
/// User context and authorization are handled by pipeline behaviors.
/// </summary>
/// <param name="userContext">The current authenticated user context.</param>
/// <param name="familyService">Service for family operations via anti-corruption layer.</param>
/// <param name="userLookupService">Service for cross-module user lookups.</param>
/// <param name="invitationRepository">Repository for invitation data access.</param>
/// <param name="unitOfWork">Unit of work for database transactions.</param>
/// <param name="subscriptionPublisher">Publisher for real-time subscription events.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class InviteFamilyMemberByEmailCommandHandler(
    IUserContext userContext,
    IFamilyService familyService,
    IUserLookupService userLookupService,
    IFamilyMemberInvitationRepository invitationRepository,
    IUnitOfWork unitOfWork,
    FamilyHub.Modules.Auth.Application.Services.SubscriptionEventPublisher subscriptionPublisher,
    ILogger<InviteFamilyMemberByEmailCommandHandler> logger)
    : ICommandHandler<InviteFamilyMemberByEmailCommand, FamilyHub.SharedKernel.Domain.Result<InviteFamilyMemberByEmailResult>>
{
    /// <inheritdoc />
    public async Task<FamilyHub.SharedKernel.Domain.Result<InviteFamilyMemberByEmailResult>> Handle(
        InviteFamilyMemberByEmailCommand request,
        CancellationToken cancellationToken)
    {
        // Get user context (already loaded and validated by behaviors)
        var currentUserId = userContext.UserId;
        LogInvitingMemberToFamily(request.Email.Value, request.FamilyId.Value, currentUserId.Value);

        // 1. Validate family exists via service
        var familyExists = await familyService.FamilyExistsAsync(request.FamilyId, cancellationToken);
        if (!familyExists)
        {
            LogFamilyNotFound(request.FamilyId.Value);
            return Result.Failure<InviteFamilyMemberByEmailResult>("Family not found.");
        }

        // 2. Check if email is already a family member (cross-module via service)
        var isExistingMember = await userLookupService.IsEmailMemberOfFamilyAsync(request.FamilyId, request.Email, cancellationToken);
        if (isExistingMember)
        {
            LogEmailAlreadyMember(request.Email.Value);
            return Result.Failure<InviteFamilyMemberByEmailResult>($"Email '{request.Email.Value}' is already a member of this family.");
        }

        // 3. Check for duplicate pending invitation using Specification pattern
        var existingInvitation = await invitationRepository.FindOneAsync(
            new PendingInvitationByEmailSpecification(request.FamilyId, request.Email),
            cancellationToken);
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
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
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

        // 7. Publish subscription event for real-time UI updates
        await subscriptionPublisher.PublishInvitationAddedAsync(
            invitation.FamilyId,
            new Presentation.GraphQL.Types.PendingInvitationType
            {
                Id = invitation.Id.Value,
                Email = invitation.Email.Value,
                Role = invitation.Role == FamilyRole.Owner ? Presentation.GraphQL.Types.UserRoleType.OWNER :
                       invitation.Role == FamilyRole.Admin ? Presentation.GraphQL.Types.UserRoleType.ADMIN :
                       invitation.Role == FamilyRole.Child ? Presentation.GraphQL.Types.UserRoleType.CHILD :
                       Presentation.GraphQL.Types.UserRoleType.MEMBER,
                Status = Presentation.GraphQL.Types.InvitationStatusType.PENDING,
                InvitedById = currentUserId.Value,
                InvitedAt = invitation.CreatedAt,
                ExpiresAt = invitation.ExpiresAt,
                Message = invitation.Message,
                DisplayCode = invitation.DisplayCode.Value
            },
            cancellationToken
        );

        // 8. Return result
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
