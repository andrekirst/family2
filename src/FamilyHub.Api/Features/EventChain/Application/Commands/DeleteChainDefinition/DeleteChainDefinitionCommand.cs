using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.DeleteChainDefinition;

public sealed record DeleteChainDefinitionCommand(
    ChainDefinitionId Id,
    FamilyId FamilyId
) : ICommand<DeleteChainDefinitionResult>, IFamilyScoped;

public sealed record DeleteChainDefinitionResult(bool Success);
