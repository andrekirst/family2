using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.EnableChainDefinition;

public sealed class EnableChainDefinitionCommandHandler(
    IChainDefinitionRepository repository,
    TimeProvider timeProvider)
    : ICommandHandler<EnableChainDefinitionCommand, EnableChainDefinitionResult>
{
    public async ValueTask<EnableChainDefinitionResult> Handle(
        EnableChainDefinitionCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var definition = await repository.GetByIdWithStepsAsync(command.Id, cancellationToken)
            ?? throw new DomainException("Chain definition not found", DomainErrorCodes.ChainDefinitionNotFound);

        definition.Enable(utcNow);
        await repository.UpdateAsync(definition, cancellationToken);

        return new EnableChainDefinitionResult(definition.Id, definition);
    }
}
