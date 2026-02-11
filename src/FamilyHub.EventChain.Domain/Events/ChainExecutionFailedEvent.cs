using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.EventChain.Domain.Events;

public sealed record ChainExecutionFailedEvent(
    ChainExecutionId ChainExecutionId,
    ChainDefinitionId ChainDefinitionId,
    FamilyId FamilyId,
    Guid CorrelationId,
    string FailedStepAlias,
    string ErrorMessage
) : DomainEvent;
