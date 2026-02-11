using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands;

public sealed record DeleteChainDefinitionCommand(
    ChainDefinitionId Id
) : ICommand<DeleteChainDefinitionResult>;

public sealed record DeleteChainDefinitionResult(bool Success);
