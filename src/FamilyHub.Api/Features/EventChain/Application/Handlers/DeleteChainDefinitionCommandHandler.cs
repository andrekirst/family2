using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.EventChain.Application.Commands;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Handlers;

public static class DeleteChainDefinitionCommandHandler
{
    public static async Task<DeleteChainDefinitionResult> Handle(
        DeleteChainDefinitionCommand command,
        IChainDefinitionRepository repository,
        CancellationToken ct)
    {
        var definition = await repository.GetByIdAsync(command.Id, ct)
            ?? throw new DomainException("Chain definition not found");

        await repository.DeleteAsync(definition, ct);
        await repository.SaveChangesAsync(ct);

        return new DeleteChainDefinitionResult(true);
    }
}
