using FamilyHub.Modules.Family.Application.Commands.InviteFamilyMemberByEmail;
using FamilyHub.Modules.Family.Application.Commands.InviteFamilyMembers;
using FamilyHub.Modules.Family.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.Family.Presentation.GraphQL.Payloads;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Domain.Exceptions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Presentation.GraphQL.Errors;
using HotChocolate;
using HotChocolate.Types;
using MediatR;

namespace FamilyHub.Modules.Family.Presentation.GraphQL.Namespaces;

/// <summary>
/// GraphQL mutations for family-related operations.
/// Extends the FamilyMutations namespace type.
/// </summary>
/// <remarks>
/// <para>
/// Uses HotChocolate mutation conventions for consistent error handling.
/// All mutations automatically include error union types via [Error] attributes.
/// </para>
/// <para>
/// Access pattern: mutation { family { inviteMember(...) { data { ... } errors { ... } } } }
/// </para>
/// <para>
/// Authorization is applied via IRequireOwnerOrAdminRole interface which requires
/// the current user to be an Owner or Admin of the family.
/// </para>
/// </remarks>
[ExtendObjectType(typeof(FamilyMutations))]
public sealed class FamilyMutationsExtensions : IRequireOwnerOrAdminRole
{
    /// <summary>
    /// Invites a family member via email with token-based invitation.
    /// Requires OWNER or ADMIN role.
    /// </summary>
    [GraphQLDescription("Invite a family member via email. Sends invitation email with token.")]
    [Error<ValidationError>]
    [Error<BusinessError>]
    [Error<ConflictError>]
    public async Task<InviteFamilyMemberByEmailResult> InviteMemberByEmail(
        Guid familyId,
        string email,
        string role,
        string? message,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new InviteFamilyMemberByEmailCommand(
            FamilyId: FamilyId.From(familyId),
            Email: Email.From(email),
            Role: FamilyRole.From(role),
            Message: message);

        var result = await mediator.Send<SharedKernel.Domain.Result<InviteFamilyMemberByEmailResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("INVITATION_FAILED", result.Error);
        }

        return result.Value;
    }

    /// <summary>
    /// Invites multiple family members via email in a single batch operation.
    /// Supports partial success - valid invitations succeed, invalid ones return errors.
    /// Maximum 20 invitations per batch.
    /// Requires OWNER or ADMIN role.
    /// </summary>
    [GraphQLDescription("Invite multiple family members in batch. Supports partial success.")]
    [Error<ValidationError>]
    [Error<BusinessError>]
    public async Task<InviteFamilyMembersResult> InviteMembers(
        InviteFamilyMembersInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var invitations = input.Invitations
            .Select(i => new InvitationRequest(
                Email: Email.From(i.Email),
                Role: FamilyRole.From(i.Role)))
            .ToList();

        var command = new InviteFamilyMembersCommand(
            FamilyId: FamilyId.From(input.FamilyId),
            Invitations: invitations,
            Message: input.Message);

        var result = await mediator.Send<InviteFamilyMembersResult>(command, cancellationToken);

        return result;
    }

    // NOTE: CancelInvitation and UpdateInvitationRole commands are currently in Auth module.
    // These should be migrated to Family module in a future phase for proper bounded context.
    // For now, these mutations remain at root level via Auth.InvitationMutations until migrated.
}
