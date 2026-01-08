using FamilyHub.Modules.Family.Application.Commands.InviteFamilyMemberByEmail;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.Exceptions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Presentation.GraphQL.Errors;
using HotChocolate.Authorization;
using MediatR;

namespace FamilyHub.Modules.Family.Presentation.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for family member invitation operations.
/// PHASE 4: Extracted from Auth module - contains mutations that operate on Family domain.
///
/// NOTE: Input/output types are still defined in Auth module to avoid circular dependencies.
/// Auth module (which already references Family module) will use these mutations.
/// </summary>
[ExtendObjectType("Mutation")]
public sealed class InvitationMutations
{
    /// <summary>
    /// Invites a family member via email with token-based invitation.
    /// Requires OWNER or ADMIN role.
    ///
    /// Input/Output types: Defined in Auth.Presentation.GraphQL (temporarily) to avoid circular dependency.
    /// Auth module's GraphQL layer will map to/from these using Family domain commands.
    /// </summary>
    [Authorize(Policy = "RequireOwnerOrAdmin")]
    [UseMutationConvention]
    [Error(typeof(BusinessError))]
    [Error(typeof(ValidationError))]
    [Error(typeof(ValueObjectError))]
    [Error(typeof(UnauthorizedError))]
    [Error(typeof(InternalServerError))]
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

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("INVITATION_FAILED", result.Error);
        }

        // Return the result directly (contains value objects)
        return result.Value;
    }
}
