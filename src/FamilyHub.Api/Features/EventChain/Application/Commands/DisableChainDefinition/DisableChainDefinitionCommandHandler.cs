using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.DisableChainDefinition;

public sealed class DisableChainDefinitionCommandHandler(
    IChainDefinitionRepository repository)
    : ICommandHandler<DisableChainDefinitionCommand, ChainDefinitionId>
{
    public async ValueTask<ChainDefinitionId> Handle(
        DisableChainDefinitionCommand command,
        CancellationToken cancellationToken)
    {
        var definition = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new DomainException("Chain definition not found", DomainErrorCodes.ChainDefinitionNotFound);

        definition.Disable();
        await repository.UpdateAsync(definition, cancellationToken);

        return definition.Id;
    }
}
