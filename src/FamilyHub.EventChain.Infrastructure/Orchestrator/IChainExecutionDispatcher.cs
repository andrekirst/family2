using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.EventChain.Infrastructure.Orchestrator;

/// <summary>
/// Abstraction for dispatching chain execution.
/// Decouples the orchestrator from the transport mechanism (CAP, in-process, etc.).
/// The orchestrator calls Dispatch after creating the execution record;
/// the implementation decides HOW to run it (publish to CAP, Task.Run, etc.).
/// </summary>
public interface IChainExecutionDispatcher
{
    Task DispatchAsync(ChainExecutionId executionId, ChainDefinitionId definitionId, CancellationToken cancellationToken = default);
}
