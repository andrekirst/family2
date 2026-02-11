using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.EventChain.Application.Commands;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Infrastructure.Orchestrator;

namespace FamilyHub.Api.Features.EventChain.Application.Handlers;

public static class ExecuteChainCommandHandler
{
    public static async Task<ExecuteChainResult> Handle(
        ExecuteChainCommand command,
        IChainDefinitionRepository definitionRepository,
        IChainExecutionRepository executionRepository,
        IChainOrchestrator orchestrator,
        CancellationToken ct)
    {
        var definition = await definitionRepository.GetByIdWithStepsAsync(command.ChainDefinitionId, ct)
            ?? throw new DomainException("Chain definition not found");

        var execution = ChainExecution.Start(
            definition.Id,
            command.FamilyId,
            definition.TriggerEventType,
            Guid.NewGuid(), // Manual execution uses a synthetic event ID
            command.TriggerPayload);

        // Create step executions
        foreach (var step in definition.Steps.OrderBy(s => s.StepOrder))
        {
            var stepExecution = StepExecution.Create(
                execution.Id,
                step.Alias.Value,
                step.Name,
                step.ActionType,
                step.StepOrder);

            execution.AddStepExecution(stepExecution);
        }

        await executionRepository.AddAsync(execution, ct);
        await executionRepository.SaveChangesAsync(ct);

        // Execute the chain
        _ = Task.Run(() => orchestrator.ExecuteChainAsync(execution, definition, ct), ct);

        return new ExecuteChainResult(execution.Id);
    }
}
