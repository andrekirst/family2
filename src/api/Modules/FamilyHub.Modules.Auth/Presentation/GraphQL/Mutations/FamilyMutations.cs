using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Presentation.GraphQL.Errors;
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
    [UseMutationConvention]
    [Error(typeof(BusinessError))]
    [Error(typeof(ValidationError))]
    [Error(typeof(ValueObjectError))]
    [Error(typeof(UnauthorizedError))]
    [Error(typeof(InternalServerError))]
    public async Task<CreatedFamilyDto> CreateFamily(
        CreateFamilyInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateFamilyCommand(FamilyName.From(input.Name));
        var result = await mediator.Send(command, cancellationToken);

        // Map result → return DTO directly
        return new CreatedFamilyDto
        {
            Id = result.FamilyId.Value,
            Name = result.Name.Value,
            OwnerId = result.OwnerId.Value,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.CreatedAt // New family, same as CreatedAt
        };
    }
}
