using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.DisableChainDefinition;

public sealed class DisableChainDefinitionCommandHandler(
    IChainDefinitionRepository repository,
    TimeProvider timeProvider)
    : ICommandHandler<DisableChainDefinitionCommand, Result<DisableChainDefinitionResult>>
{
    public async ValueTask<Result<DisableChainDefinitionResult>> Handle(
        DisableChainDefinitionCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var definition = await repository.GetByIdWithStepsAsync(command.Id, cancellationToken);

        if (definition is null)
        {
            return DomainError.NotFound(DomainErrorCodes.ChainDefinitionNotFound, "Chain definition not found");
        }

        definition.Disable(utcNow);
        await repository.UpdateAsync(definition, cancellationToken);

        return new DisableChainDefinitionResult(definition.Id, definition);
    }
}
