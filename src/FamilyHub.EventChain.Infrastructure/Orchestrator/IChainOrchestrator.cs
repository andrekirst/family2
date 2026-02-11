using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Entities;

namespace FamilyHub.EventChain.Infrastructure.Orchestrator;

public interface IChainOrchestrator
{
    Task TryTriggerChainsAsync(IDomainEvent domainEvent, CancellationToken ct = default);
    Task ExecuteChainAsync(ChainExecution execution, ChainDefinition definition, CancellationToken ct = default);
    Task ResumeStepAsync(Guid stepExecutionId, CancellationToken ct = default);
}
