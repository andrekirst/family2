using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation;
using FamilyHub.Modules.Auth.Application.Commands.CancelInvitation;
using FamilyHub.Modules.Auth.Application.Commands.InviteFamilyMemberByEmail;
using FamilyHub.Modules.Auth.Application.Commands.UpdateInvitationRole;
// using FamilyHub.Modules.Auth.Application.Commands.ResendInvitation; // TODO: Implement ResendInvitation command
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Mappers;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Domain.Exceptions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Presentation.GraphQL.Errors;
using HotChocolate.Authorization;
using MediatR;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for family member invitation operations.
/// </summary>
[ExtendObjectType("Mutation")]
public sealed class InvitationMutations
{
    /// <summary>
    /// Invites a family member via email with token-based invitation.
    /// Requires OWNER or ADMIN role.
    /// </summary>
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [UseMutationConvention]
    [Error(typeof(BusinessError))]
    [Error(typeof(ValidationError))]
    [Error(typeof(ValueObjectError))]
    [Error(typeof(UnauthorizedError))]
    [Error(typeof(InternalServerError))]
    public async Task<PendingInvitationType> InviteFamilyMemberByEmail(
        InviteFamilyMemberByEmailInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        // Map input → command (primitives → value objects)
        var command = new InviteFamilyMemberByEmailCommand(
            FamilyId: FamilyId.From(input.FamilyId),
            Email: Email.From(input.Email),
            Role: FamilyRole.From(input.Role),
            Message: input.Message
        );

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            // FIXED: Throw BusinessException with proper error code instead of generic Exception
            throw new BusinessException("INVITATION_FAILED", result.Error);
        }

        // Map result → return DTO directly
        return new PendingInvitationType
        {
            Id = result.Value.InvitationId.Value,
            Email = result.Value.Email.Value,
            Role = result.Value.Role.AsRoleType(),
            Status = result.Value.Status.AsStatusType(),
            InvitedAt = result.Value.ExpiresAt.AddDays(-14), // Calculate from ExpiresAt (invitations expire after 14 days)
            ExpiresAt = result.Value.ExpiresAt,
            DisplayCode = result.Value.DisplayCode.Value
        };
    }


    /// <summary>
    /// Cancels a pending invitation.
    /// Requires OWNER or ADMIN role.
    /// </summary>
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [UseMutationConvention]
    [Error(typeof(BusinessError))]
    [Error(typeof(ValidationError))]
    [Error(typeof(ValueObjectError))]
    [Error(typeof(UnauthorizedError))]
    [Error(typeof(InternalServerError))]
    public async Task<bool> CancelInvitation(
        CancelInvitationInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CancelInvitationCommand(
            InvitationId: InvitationId.From(input.InvitationId)
        );

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            // FIXED: Throw BusinessException with proper error code instead of generic Exception
            throw new BusinessException("CANCELLATION_FAILED", result.Error);
        }

        // Return success indicator
        return true;
    }

    // TODO: Implement ResendInvitation mutation after creating command
    // [Authorize(Policy = "RequireOwnerOrAdmin")]
    // public async Task<ResendInvitationPayload> ResendInvitation(...)

    /// <summary>
    /// Updates the role of a pending invitation.
    /// Requires OWNER or ADMIN role.
    /// </summary>
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [UseMutationConvention]
    [Error(typeof(BusinessError))]
    [Error(typeof(ValidationError))]
    [Error(typeof(ValueObjectError))]
    [Error(typeof(UnauthorizedError))]
    [Error(typeof(InternalServerError))]
    public async Task<UpdatedInvitationDto> UpdateInvitationRole(
        UpdateInvitationRoleInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateInvitationRoleCommand(
            InvitationId: InvitationId.From(input.InvitationId),
            NewRole: FamilyRole.From(input.NewRole)
        );

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            // FIXED: Throw BusinessException with proper error code instead of generic Exception
            throw new BusinessException("UPDATE_ROLE_FAILED", result.Error);
        }

        // Map result → return DTO directly
        return new UpdatedInvitationDto
        {
            InvitationId = result.Value.InvitationId.Value,
            Role = result.Value.Role.AsRoleType()
        };
    }

    /// <summary>
    /// Accepts a family invitation using a token.
    /// No authorization required - allows invitees to join without being authenticated members yet.
    /// </summary>
    [Authorize] // User must be authenticated (but doesn't need to be a family member yet)
    [UseMutationConvention]
    [Error(typeof(BusinessError))]
    [Error(typeof(ValidationError))]
    [Error(typeof(ValueObjectError))]
    [Error(typeof(UnauthorizedError))]
    [Error(typeof(InternalServerError))]
    public async Task<AcceptedInvitationDto> AcceptInvitation(
        AcceptInvitationInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new AcceptInvitationCommand(
            Token: InvitationToken.From(input.Token)
        );

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            // FIXED: Throw BusinessException with proper error code instead of generic Exception
            throw new BusinessException("ACCEPT_INVITATION_FAILED", result.Error);
        }

        // Map result → return DTO directly
        return new AcceptedInvitationDto
        {
            FamilyId = result.Value.FamilyId.Value,
            FamilyName = result.Value.FamilyName.Value,
            Role = result.Value.Role.AsRoleType()
        };
    }
}

/// <summary>
/// DTO for updated invitation information.
/// </summary>
public sealed record UpdatedInvitationDto
{
    public required Guid InvitationId { get; init; }
    public required UserRoleType Role { get; init; }
}

/// <summary>
/// DTO for accepted invitation information.
/// </summary>
public sealed record AcceptedInvitationDto
{
    public required Guid FamilyId { get; init; }
    public required string FamilyName { get; init; }
    public required UserRoleType Role { get; init; }
}
