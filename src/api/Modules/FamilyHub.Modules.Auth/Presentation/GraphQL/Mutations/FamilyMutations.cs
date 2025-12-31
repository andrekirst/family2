using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Presentation.GraphQL;
using MediatR;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for family operations.
/// </summary>
[ExtendObjectType("Mutation")]
public sealed class FamilyMutations
{
    /// <summary>
    /// Creates a new family with the authenticated user as owner.
    /// Authentication is validated in the handler via ICurrentUserService.
    /// Uses MediatR for command validation and execution.
    /// </summary>
    public async Task<CreateFamilyPayload> CreateFamily(
        CreateFamilyInput input,
        [Service] IMutationHandler mutationHandler,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mutationHandler.Handle<CreateFamilyResult, CreateFamilyPayload>(async () =>
        {
            var command = new CreateFamilyCommand(FamilyName.From(input.Name));
            return await mediator.Send(command, cancellationToken);
        });
    }
}
