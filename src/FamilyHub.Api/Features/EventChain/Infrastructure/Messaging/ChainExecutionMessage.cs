namespace FamilyHub.Api.Features.EventChain.Infrastructure.Messaging;

/// <summary>
/// CAP message payload for dispatching chain execution.
/// Contains only IDs — the subscriber resolves full entities from the database.
/// </summary>
public sealed record ChainExecutionMessage(Guid ExecutionId, Guid DefinitionId);

/// <summary>
/// CAP message payload for scheduled step execution.
/// </summary>
public sealed record ScheduledStepMessage(Guid StepExecutionId);
