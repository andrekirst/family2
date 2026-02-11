using FamilyHub.Api.Features.EventChain.Application.Commands;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Infrastructure.Registry;

namespace FamilyHub.Api.Features.EventChain.Application.Handlers;

public static class CreateChainDefinitionCommandHandler
{
    public static async Task<CreateChainDefinitionResult> Handle(
        CreateChainDefinitionCommand command,
        IChainDefinitionRepository repository,
        IChainRegistry registry,
        CancellationToken ct)
    {
        // Validate trigger exists in registry
        if (!registry.IsValidTrigger(command.TriggerEventType))
            throw new InvalidOperationException($"Unknown trigger event type: {command.TriggerEventType}");

        var trigger = registry.GetTrigger(command.TriggerEventType)!;

        var definition = ChainDefinition.Create(
            command.Name,
            command.Description,
            command.FamilyId,
            command.CreatedByUserId,
            command.TriggerEventType,
            trigger.Module,
            trigger.Description,
            trigger.OutputSchema);

        // Add steps
        foreach (var stepCmd in command.Steps.OrderBy(s => s.Order))
        {
            if (!registry.IsValidAction(stepCmd.ActionType, stepCmd.ActionVersion.Value))
                throw new InvalidOperationException(
                    $"Unknown action: {stepCmd.ActionType}@{stepCmd.ActionVersion.Value}");

            var action = registry.GetAction(stepCmd.ActionType, stepCmd.ActionVersion.Value)!;

            var step = ChainDefinitionStep.Create(
                definition.Id,
                stepCmd.Alias,
                stepCmd.Name,
                stepCmd.ActionType,
                stepCmd.ActionVersion,
                action.Module,
                stepCmd.InputMappings,
                stepCmd.Condition,
                action.IsCompensatable,
                null, // compensation action type determined by action descriptor
                stepCmd.Order);

            definition.AddStep(step);
        }

        await repository.AddAsync(definition, ct);
        await repository.SaveChangesAsync(ct);

        return new CreateChainDefinitionResult(definition.Id);
    }
}
