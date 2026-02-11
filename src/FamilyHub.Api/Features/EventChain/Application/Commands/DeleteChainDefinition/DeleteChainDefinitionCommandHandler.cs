using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.DeleteChainDefinition;

public sealed class DeleteChainDefinitionCommandHandler(
    IChainDefinitionRepository repository)
    : ICommandHandler<DeleteChainDefinitionCommand, DeleteChainDefinitionResult>
{
    public async ValueTask<DeleteChainDefinitionResult> Handle(
        DeleteChainDefinitionCommand command,
        CancellationToken cancellationToken)
    {
        var definition = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new DomainException("Chain definition not found");

        await repository.DeleteAsync(definition, cancellationToken);

        return new DeleteChainDefinitionResult(true);
    }
}
