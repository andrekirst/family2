using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Entities;

namespace FamilyHub.EventChain.Infrastructure.Orchestrator;

public interface IChainOrchestrator
{
    Task TryTriggerChainsAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task ExecuteChainAsync(ChainExecution execution, ChainDefinition definition, CancellationToken cancellationToken = default);
    Task ResumeStepAsync(Guid stepExecutionId, CancellationToken cancellationToken = default);
}
