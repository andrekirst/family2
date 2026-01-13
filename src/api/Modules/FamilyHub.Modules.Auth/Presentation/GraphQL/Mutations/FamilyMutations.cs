using FamilyHub.Infrastructure.GraphQL.Interceptors;
using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for family operations.
/// Authorization is applied via <see cref="IRequireAuthentication"/> interface.
/// </summary>
[ExtendObjectType("Mutation")]
public sealed class FamilyMutations : IRequireAuthentication
{
    /// <summary>
    /// Creates a new family with the authenticated user as owner.
    /// Requires authentication (via class-level IRequireAuthentication).
    /// Uses MediatR for command validation and execution.
    /// </summary>
    [DefaultMutationErrors]
    [UseMutationConvention]
    public async Task<CreatedFamilyDto> CreateFamily(
        CreateFamilyInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateFamilyCommand(FamilyName.From(input.Name));

        // Explicit type parameter needed because C# can't infer TResponse through ICommand<T> : IRequest<T>
        var result = await mediator.Send<CreateFamilyResult>(command, cancellationToken);

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
