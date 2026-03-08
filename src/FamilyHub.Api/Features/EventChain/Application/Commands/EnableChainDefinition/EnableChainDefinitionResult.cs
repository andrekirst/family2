using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.EnableChainDefinition;

public sealed record EnableChainDefinitionResult(
    ChainDefinitionId ChainDefinitionId,
    ChainDefinition Definition
);
