using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.Enums;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.EventChain.Domain.Events;

public sealed record ChainExecutionCompletedEvent(
    ChainExecutionId ChainExecutionId,
    ChainDefinitionId ChainDefinitionId,
    FamilyId FamilyId,
    Guid CorrelationId,
    ChainExecutionStatus FinalStatus,
    int CompletedSteps,
    int TotalSteps
) : DomainEvent;
