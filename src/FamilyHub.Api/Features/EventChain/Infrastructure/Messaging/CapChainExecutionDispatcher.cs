using DotNetCore.CAP;
using FamilyHub.EventChain.Domain.ValueObjects;
using FamilyHub.EventChain.Infrastructure.Orchestrator;

namespace FamilyHub.Api.Features.EventChain.Infrastructure.Messaging;

/// <summary>
/// CAP-based implementation of IChainExecutionDispatcher.
/// Publishes a message to the chain.execution.run topic so that
/// chain execution survives process crashes (transactional outbox).
/// </summary>
public sealed class CapChainExecutionDispatcher(
    ICapPublisher capPublisher,
    ILogger<CapChainExecutionDispatcher> logger) : IChainExecutionDispatcher
{
    public async Task DispatchAsync(
        ChainExecutionId executionId,
        ChainDefinitionId definitionId,
        CancellationToken cancellationToken = default)
    {
        var message = new ChainExecutionMessage(executionId.Value, definitionId.Value);

        await capPublisher.PublishAsync(
            ChainCapTopics.ExecutionRun,
            message,
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Dispatched chain execution {ExecutionId} via CAP topic {Topic}",
            executionId.Value, ChainCapTopics.ExecutionRun);
    }
}
