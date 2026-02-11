using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Queries.GetChainDefinitions;

public sealed record GetChainDefinitionsQuery(
    FamilyId FamilyId,
    bool? IsEnabled = null
) : IQuery<IReadOnlyList<ChainDefinition>>;
