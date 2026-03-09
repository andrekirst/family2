using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.DisableChainDefinition;

public sealed class DisableChainDefinitionCommandHandler(
    IChainDefinitionRepository repository,
    TimeProvider timeProvider)
    : ICommandHandler<DisableChainDefinitionCommand, DisableChainDefinitionResult>
{
    public async ValueTask<DisableChainDefinitionResult> Handle(
        DisableChainDefinitionCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var definition = await repository.GetByIdWithStepsAsync(command.Id, cancellationToken)
            ?? throw new DomainException("Chain definition not found", DomainErrorCodes.ChainDefinitionNotFound);

        definition.Disable(utcNow);
        await repository.UpdateAsync(definition, cancellationToken);

        return new DisableChainDefinitionResult(definition.Id, definition);
    }
}
