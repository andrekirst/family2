using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.DisableChainDefinition;

public sealed record DisableChainDefinitionCommand(ChainDefinitionId Id) : ICommand<ChainDefinitionId>;
