using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.EnableChainDefinition;

public sealed class EnableChainDefinitionCommandHandler(
    IChainDefinitionRepository repository)
    : ICommandHandler<EnableChainDefinitionCommand, ChainDefinitionId>
{
    public async ValueTask<ChainDefinitionId> Handle(
        EnableChainDefinitionCommand command,
        CancellationToken cancellationToken)
    {
        var definition = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new DomainException("Chain definition not found");

        definition.Enable();
        await repository.UpdateAsync(definition, cancellationToken);

        return definition.Id;
    }
}
