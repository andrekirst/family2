using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.EventChain.Application.Commands.CreateChainDefinition;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.UpdateChainDefinition;

public sealed record UpdateChainDefinitionCommand(
    ChainDefinitionId Id,
    ChainName? Name,
    string? Description,
    bool? IsEnabled,
    IReadOnlyList<CreateStepCommand>? Steps,
    FamilyId FamilyId
) : ICommand<UpdateChainDefinitionResult>, IFamilyScoped;

public sealed record UpdateChainDefinitionResult(ChainDefinitionId ChainDefinitionId);
