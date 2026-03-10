using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Enums;
using FamilyHub.EventChain.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Queries.GetChainExecutions;

public sealed record GetChainExecutionsQuery(
    ChainDefinitionId? ChainDefinitionId = null,
    ChainExecutionStatus? Status = null
) : IReadOnlyQuery<IReadOnlyList<ChainExecution>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
