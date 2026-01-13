using FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation;
using FamilyHub.Modules.Auth.Application.Commands.CancelInvitation;
using FamilyHub.Modules.Auth.Application.Commands.UpdateInvitationRole;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Mappers;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Domain.Exceptions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Presentation.GraphQL.Errors;
using HotChocolate.Authorization;
using MediatR;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for family member invitation operations.
///
/// PHASE 4 UPDATE: InviteFamilyMemberByEmail mutation moved to Family.Presentation.GraphQL.Mutations.
/// This class now contains only mutations that modify User aggregate:
/// - CancelInvitation (temporarily, until Phase 5+)
/// - UpdateInvitationRole (temporarily, until Phase 5+)
/// - AcceptInvitation (modifies User to add family membership)
/// </summary>
[ExtendObjectType("Mutation")]
public sealed class InvitationMutations
{
    // NOTE: InviteFamilyMemberByEmail moved to Family.Presentation.GraphQL.Mutations.InvitationMutations
    // It uses Family.Application.Commands.InviteFamilyMemberByEmailCommand


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
    /// <summary>
    /// Gets the unique identifier of the updated invitation.
    /// </summary>
    public required Guid InvitationId { get; init; }

    /// <summary>
    /// Gets the newly assigned role for the invitation.
    /// </summary>
    public required UserRoleType Role { get; init; }
}

/// <summary>
/// DTO for accepted invitation information.
/// </summary>
public sealed record AcceptedInvitationDto
{
    /// <summary>
    /// Gets the unique identifier of the family joined.
    /// </summary>
    public required Guid FamilyId { get; init; }

    /// <summary>
    /// Gets the name of the family joined.
    /// </summary>
    public required string FamilyName { get; init; }

    /// <summary>
    /// Gets the role assigned to the user in the family.
    /// </summary>
    public required UserRoleType Role { get; init; }
}
