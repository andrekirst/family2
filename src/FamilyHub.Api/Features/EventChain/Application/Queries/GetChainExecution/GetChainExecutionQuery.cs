using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Queries.GetChainExecution;

public sealed record GetChainExecutionQuery(
    ChainExecutionId Id,
    FamilyId FamilyId
) : IReadOnlyQuery<ChainExecution?>, IFamilyScoped;
