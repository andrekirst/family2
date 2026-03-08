using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Queries.GetChainDefinition;

public sealed record GetChainDefinitionQuery(
    ChainDefinitionId Id,
    FamilyId FamilyId
) : IReadOnlyQuery<ChainDefinition?>, IFamilyScoped;
