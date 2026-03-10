using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.DisableChainDefinition;

public sealed record DisableChainDefinitionResult(
    ChainDefinitionId ChainDefinitionId,
    ChainDefinition Definition
);
