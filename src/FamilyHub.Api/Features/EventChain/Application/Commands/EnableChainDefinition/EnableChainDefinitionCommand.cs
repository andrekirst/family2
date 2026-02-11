using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.EnableChainDefinition;

public sealed record EnableChainDefinitionCommand(ChainDefinitionId Id) : ICommand<ChainDefinitionId>;
