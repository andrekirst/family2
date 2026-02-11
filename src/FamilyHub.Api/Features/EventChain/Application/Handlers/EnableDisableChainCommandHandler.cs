using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.EventChain.Application.Commands;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Handlers;

public static class EnableChainDefinitionCommandHandler
{
    public static async Task<ChainDefinitionId> Handle(
        EnableChainDefinitionCommand command,
        IChainDefinitionRepository repository,
        CancellationToken ct)
    {
        var definition = await repository.GetByIdAsync(command.Id, ct)
            ?? throw new DomainException("Chain definition not found");

        definition.Enable();
        await repository.UpdateAsync(definition, ct);
        await repository.SaveChangesAsync(ct);

        return definition.Id;
    }
}

public static class DisableChainDefinitionCommandHandler
{
    public static async Task<ChainDefinitionId> Handle(
        DisableChainDefinitionCommand command,
        IChainDefinitionRepository repository,
        CancellationToken ct)
    {
        var definition = await repository.GetByIdAsync(command.Id, ct)
            ?? throw new DomainException("Chain definition not found");

        definition.Disable();
        await repository.UpdateAsync(definition, ct);
        await repository.SaveChangesAsync(ct);

        return definition.Id;
    }
}
