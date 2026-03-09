using DotNetCore.CAP;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Infrastructure.Orchestrator;

namespace FamilyHub.Api.Features.EventChain.Infrastructure.Messaging;

/// <summary>
/// CAP subscriber for Event Chain messages.
/// Handles chain execution dispatched via the transactional outbox.
/// CAP provides at-least-once delivery with automatic retry on failure.
/// </summary>
public sealed class ChainCapSubscriber(
    IChainOrchestrator orchestrator,
    IChainDefinitionRepository definitionRepository,
    IChainExecutionRepository executionRepository,
    ILogger<ChainCapSubscriber> logger) : ICapSubscribe
{
    /// <summary>
    /// Handles chain execution requests published by CapChainExecutionDispatcher.
    /// Loads execution and definition from DB, then runs the chain.
    /// </summary>
    [CapSubscribe(ChainCapTopics.ExecutionRun)]
    public async Task HandleExecutionRunAsync(ChainExecutionMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "CAP subscriber received chain execution request. ExecutionId={ExecutionId}, DefinitionId={DefinitionId}",
            message.ExecutionId, message.DefinitionId);

        var execution = await executionRepository.GetByIdWithStepsAsync(
            FamilyHub.EventChain.Domain.ValueObjects.ChainExecutionId.From(message.ExecutionId),
            cancellationToken);

        if (execution is null)
        {
            logger.LogWarning("Chain execution {ExecutionId} not found — may have been deleted", message.ExecutionId);
            return;
        }

        var definition = await definitionRepository.GetByIdWithStepsAsync(
            FamilyHub.EventChain.Domain.ValueObjects.ChainDefinitionId.From(message.DefinitionId),
            cancellationToken);

        if (definition is null)
        {
            logger.LogWarning("Chain definition {DefinitionId} not found — may have been deleted", message.DefinitionId);
            return;
        }

        await orchestrator.ExecuteChainAsync(execution, definition, cancellationToken);
    }

    /// <summary>
    /// Handles scheduled step execution (replaces ChainSchedulerService polling).
    /// </summary>
    [CapSubscribe(ChainCapTopics.ScheduledStepReady)]
    public async Task HandleScheduledStepAsync(ScheduledStepMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "CAP subscriber received scheduled step ready. StepExecutionId={StepExecutionId}",
            message.StepExecutionId);

        await orchestrator.ResumeStepAsync(message.StepExecutionId, cancellationToken);
    }
}
