using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Queries.GetChainDefinition;

public sealed record GetChainDefinitionQuery(
    ChainDefinitionId Id
) : IQuery<ChainDefinition?>;
