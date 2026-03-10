using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.EnableChainDefinition;

public sealed class EnableChainDefinitionCommandHandler(
    IChainDefinitionRepository repository,
    TimeProvider timeProvider)
    : ICommandHandler<EnableChainDefinitionCommand, Result<EnableChainDefinitionResult>>
{
    public async ValueTask<Result<EnableChainDefinitionResult>> Handle(
        EnableChainDefinitionCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var definition = await repository.GetByIdWithStepsAsync(command.Id, cancellationToken);

        if (definition is null)
        {
            return DomainError.NotFound(DomainErrorCodes.ChainDefinitionNotFound, "Chain definition not found");
        }

        definition.Enable(utcNow);
        await repository.UpdateAsync(definition, cancellationToken);

        return new EnableChainDefinitionResult(definition.Id, definition);
    }
}
