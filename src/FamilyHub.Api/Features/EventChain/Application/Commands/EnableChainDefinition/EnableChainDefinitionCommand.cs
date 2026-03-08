using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.Api.Features.EventChain.Application.Commands.EnableChainDefinition;

public sealed record EnableChainDefinitionCommand(ChainDefinitionId Id, FamilyId FamilyId) : ICommand<ChainDefinitionId>, IFamilyScoped;
