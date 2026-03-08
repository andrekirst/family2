using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.DisableChainDefinition;

public sealed record DisableChainDefinitionCommand(ChainDefinitionId Id, FamilyId FamilyId) : ICommand<ChainDefinitionId>, IFamilyScoped;
