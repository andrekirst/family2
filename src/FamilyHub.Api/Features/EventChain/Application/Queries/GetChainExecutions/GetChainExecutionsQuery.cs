using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Enums;
using FamilyHub.EventChain.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Queries.GetChainExecutions;

public sealed record GetChainExecutionsQuery(
    FamilyId FamilyId,
    ChainDefinitionId? ChainDefinitionId = null,
    ChainExecutionStatus? Status = null
) : IQuery<IReadOnlyList<ChainExecution>>;
