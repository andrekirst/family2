using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Infrastructure.Orchestrator;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.ExecuteChain;

public sealed class ExecuteChainCommandHandler(
    IChainDefinitionRepository definitionRepository,
    IChainExecutionRepository executionRepository,
    IChainOrchestrator orchestrator)
    : ICommandHandler<ExecuteChainCommand, ExecuteChainResult>
{
    public async ValueTask<ExecuteChainResult> Handle(
        ExecuteChainCommand command,
        CancellationToken cancellationToken)
    {
        var definition = await definitionRepository.GetByIdWithStepsAsync(command.ChainDefinitionId, cancellationToken)
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

        await executionRepository.AddAsync(execution, cancellationToken);

        // Execute the chain asynchronously
        _ = Task.Run(() => orchestrator.ExecuteChainAsync(execution, definition, cancellationToken), cancellationToken);

        return new ExecuteChainResult(execution.Id);
    }
}
