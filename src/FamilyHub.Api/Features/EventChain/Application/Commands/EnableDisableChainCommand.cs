using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands;

public sealed record EnableChainDefinitionCommand(ChainDefinitionId Id) : ICommand<ChainDefinitionId>;
public sealed record DisableChainDefinitionCommand(ChainDefinitionId Id) : ICommand<ChainDefinitionId>;
