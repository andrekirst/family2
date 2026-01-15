using FamilyHub.Infrastructure.GraphQL.Interceptors;
using FamilyHub.Modules.Family.Application.Commands.InviteFamilyMemberByEmail;
using FamilyHub.Modules.Family.Application.Commands.InviteFamilyMembers;
using FamilyHub.Modules.Family.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.Family.Presentation.GraphQL.Payloads;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Domain.Exceptions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace FamilyHub.Modules.Family.Presentation.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for family member invitation operations.
/// PHASE 4: Extracted from Auth module - contains mutations that operate on Family domain.
///
/// NOTE: Input/output types are still defined in Auth module to avoid circular dependencies.
/// Auth module (which already references Family module) will use these mutations.
///
/// Authorization is applied via <see cref="IRequireOwnerOrAdminRole"/> interface,
/// which is enforced by <see cref="AuthorizationTypeInterceptor"/> at GraphQL level.
/// </summary>
[ExtendObjectType("Mutation")]
public sealed class InvitationMutations : IRequireOwnerOrAdminRole
{
    /// <summary>
    /// Invites a family member via email with token-based invitation.
    /// Requires OWNER or ADMIN role.
    ///
    /// Input/Output types: Defined in Auth.Presentation.GraphQL (temporarily) to avoid circular dependency.
    /// Auth module's GraphQL layer will map to/from these using Family domain commands.
    /// </summary>
    [DefaultMutationErrors]
    [UseMutationConvention]
    public async Task<InviteFamilyMemberByEmailResult> InviteFamilyMemberByEmail(
        Guid familyId,
        string email,
        string role,
        string? message,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        // Map primitives → command (value objects)
        var command = new InviteFamilyMemberByEmailCommand(
            FamilyId: FamilyId.From(familyId),
            Email: Email.From(email),
            Role: FamilyRole.From(role),
            Message: message
        );

        // Explicit type parameter needed because C# can't infer TResponse through ICommand<T> : IRequest<T>
        var result = await mediator.Send<SharedKernel.Domain.Result<InviteFamilyMemberByEmailResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("INVITATION_FAILED", result.Error);
        }

        // Return the result directly (contains value objects)
        return result.Value;
    }

    /// <summary>
    /// Invites multiple family members via email in a single batch operation.
    /// Supports partial success - valid invitations succeed, invalid ones return errors.
    /// Maximum 20 invitations per batch.
    /// Requires OWNER or ADMIN role.
    /// </summary>
    [DefaultMutationErrors]
    [UseMutationConvention]
    public async Task<InviteFamilyMembersDto> InviteFamilyMembers(
        InviteFamilyMembersInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        // Map input DTOs → command with value objects
        var invitations = input.Invitations
            .Select(i => new InvitationRequest(
                Email: Email.From(i.Email),
                Role: FamilyRole.From(i.Role)
            ))
            .ToList();

        var command = new InviteFamilyMembersCommand(
            FamilyId: FamilyId.From(input.FamilyId),
            Invitations: invitations,
            Message: input.Message
        );

        // Explicit type parameter needed because C# can't infer TResponse through ICommand<T> : IRequest<T>
        var result = await mediator.Send<InviteFamilyMembersResult>(command, cancellationToken);

        // Map result → DTO (primitives for GraphQL)
        return new InviteFamilyMembersDto
        {
            SuccessfulInvitations = result.SuccessfulInvitations
                .Select(s => new InvitationSuccessDto
                {
                    InvitationId = s.InvitationId.Value,
                    Email = s.Email.Value,
                    Role = s.Role.Value.ToUpperInvariant(),
                    Token = s.Token.Value,
                    DisplayCode = s.DisplayCode.Value,
                    ExpiresAt = s.ExpiresAt,
                    Status = s.Status.ToString()
                })
                .ToList(),
            FailedInvitations = result.FailedInvitations
                .Select(f => new InvitationFailureDto
                {
                    Email = f.Email.Value,
                    Role = f.Role.Value.ToUpperInvariant(),
                    ErrorCode = f.ErrorCode.ToString(),
                    ErrorMessage = f.ErrorMessage
                })
                .ToList()
        };
    }
}
