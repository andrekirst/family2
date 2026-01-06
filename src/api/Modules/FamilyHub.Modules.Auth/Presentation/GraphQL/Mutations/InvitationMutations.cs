using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation;
using FamilyHub.Modules.Auth.Application.Commands.CancelInvitation;
using FamilyHub.Modules.Auth.Application.Commands.InviteFamilyMemberByEmail;
using FamilyHub.Modules.Auth.Application.Commands.UpdateInvitationRole;
// using FamilyHub.Modules.Auth.Application.Commands.ResendInvitation; // TODO: Implement ResendInvitation command
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Presentation.GraphQL;
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
    public async Task<InviteFamilyMemberByEmailPayload> InviteFamilyMemberByEmail(
        InviteFamilyMemberByEmailInput input,
        [Service] IMutationHandler mutationHandler,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mutationHandler.Handle<InviteFamilyMemberByEmailResult, InviteFamilyMemberByEmailPayload>(async () =>
        {
            // Map input → command (primitives → value objects)
            var command = new InviteFamilyMemberByEmailCommand(
                FamilyId: FamilyId.From(input.FamilyId),
                Email: Email.From(input.Email),
                Role: UserRole.From(input.Role),
                Message: input.Message
            );

            return await mediator.Send(command, cancellationToken);
        });
    }


    /// <summary>
    /// Cancels a pending invitation.
    /// Requires OWNER or ADMIN role.
    /// </summary>
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    public async Task<CancelInvitationPayload> CancelInvitation(
        CancelInvitationInput input,
        [Service] IMutationHandler mutationHandler,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mutationHandler.Handle<FamilyHub.SharedKernel.Domain.Result, CancelInvitationPayload>(async () =>
        {
            var command = new CancelInvitationCommand(
                InvitationId: InvitationId.From(input.InvitationId)
            );

            return await mediator.Send(command, cancellationToken);
        });
    }

    // TODO: Implement ResendInvitation mutation after creating command
    // [Authorize(Policy = "RequireOwnerOrAdmin")]
    // public async Task<ResendInvitationPayload> ResendInvitation(...)

    /// <summary>
    /// Updates the role of a pending invitation.
    /// Requires OWNER or ADMIN role.
    /// </summary>
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    public async Task<UpdateInvitationRolePayload> UpdateInvitationRole(
        UpdateInvitationRoleInput input,
        [Service] IMutationHandler mutationHandler,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mutationHandler.Handle<UpdateInvitationRoleResult, UpdateInvitationRolePayload>(async () =>
        {
            var command = new UpdateInvitationRoleCommand(
                InvitationId: InvitationId.From(input.InvitationId),
                NewRole: UserRole.From(input.NewRole)
            );

            return await mediator.Send(command, cancellationToken);
        });
    }

    /// <summary>
    /// Accepts a family invitation using a token.
    /// No authorization required - allows invitees to join without being authenticated members yet.
    /// </summary>
    [Authorize] // User must be authenticated (but doesn't need to be a family member yet)
    public async Task<AcceptInvitationPayload> AcceptInvitation(
        AcceptInvitationInput input,
        [Service] IMutationHandler mutationHandler,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mutationHandler.Handle<AcceptInvitationResult, AcceptInvitationPayload>(async () =>
        {
            var command = new AcceptInvitationCommand(
                Token: InvitationToken.From(input.Token)
            );

            return await mediator.Send(command, cancellationToken);
        });
    }

}
