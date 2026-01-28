using FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Mappers;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Domain.Exceptions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Presentation.GraphQL.Errors;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using MediatR;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Namespaces;

/// <summary>
/// GraphQL mutations for account-related operations.
/// Extends the AccountMutations namespace type.
/// </summary>
/// <remarks>
/// <para>
/// Uses HotChocolate mutation conventions for consistent error handling.
/// All mutations automatically include error union types via [Error] attributes.
/// </para>
/// <para>
/// Access pattern: mutation { account { acceptInvitation(...) { data { ... } errors { ... } } } }
/// </para>
/// </remarks>
[ExtendObjectType(typeof(AccountMutations))]
public sealed class AccountMutationsExtensions
{
    /// <summary>
    /// Accepts a family invitation using a token.
    /// Adds the current user to the family with the role specified in the invitation.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Accept a family invitation using the token from the invitation email.")]
    [UseMutationConvention]
    [Error<ValidationError>]
    [Error<BusinessError>]
    [Error<NotFoundError>]
    public async Task<AcceptedInvitationResult> AcceptInvitation(
        AcceptInvitationInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new AcceptInvitationCommand(
            Token: InvitationToken.From(input.Token));

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<AcceptInvitationResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("ACCEPT_INVITATION_FAILED", result.Error);
        }

        return new AcceptedInvitationResult
        {
            FamilyId = result.Value.FamilyId.Value,
            FamilyName = result.Value.FamilyName.Value,
            Role = result.Value.Role.AsRoleType()
        };
    }
}

/// <summary>
/// Result of accepting a family invitation.
/// </summary>
public sealed record AcceptedInvitationResult
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
