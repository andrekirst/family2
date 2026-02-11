using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.EventChain.Application.Mappers;
using FamilyHub.Api.Features.EventChain.Application.Queries.GetChainDefinitions;
using FamilyHub.Api.Features.EventChain.Application.Queries.GetChainDefinition;
using FamilyHub.Api.Features.EventChain.Application.Queries.GetChainExecutions;
using FamilyHub.Api.Features.EventChain.Application.Queries.GetChainExecution;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Enums;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Domain.ValueObjects;
using FamilyHub.EventChain.Infrastructure.Registry;
using FamilyHub.Api.Features.EventChain.Models;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.EventChain.GraphQL;

[ExtendObjectType(typeof(EventChainQuery))]
public class ChainQueries
{
    [Authorize]
    public async Task<IReadOnlyList<ChainDefinitionDto>> GetChainDefinitions(
        Guid familyId,
        bool? isEnabled,
        [Service] IQueryBus queryBus,
        [Service] IChainExecutionRepository executionRepository,
        CancellationToken ct)
    {
        var query = new GetChainDefinitionsQuery(FamilyId.From(familyId), isEnabled);
        var definitions = await queryBus.QueryAsync<IReadOnlyList<ChainDefinition>>(query, ct);

        var result = new List<ChainDefinitionDto>();
        foreach (var def in definitions)
        {
            var count = await executionRepository.GetExecutionCountAsync(def.Id, ct);
            var lastExec = await executionRepository.GetLastExecutedAtAsync(def.Id, ct);
            result.Add(ChainMapper.ToDto(def, count, lastExec));
        }

        return result;
    }

    [Authorize]
    public async Task<ChainDefinitionDto?> GetChainDefinition(
        Guid id,
        [Service] IQueryBus queryBus,
        [Service] IChainExecutionRepository executionRepository,
        CancellationToken ct)
    {
        var query = new GetChainDefinitionQuery(ChainDefinitionId.From(id));
        var definition = await queryBus.QueryAsync<ChainDefinition?>(query, ct);

        if (definition is null) return null;

        var count = await executionRepository.GetExecutionCountAsync(definition.Id, ct);
        var lastExec = await executionRepository.GetLastExecutedAtAsync(definition.Id, ct);
        return ChainMapper.ToDto(definition, count, lastExec);
    }

    [Authorize]
    public async Task<IReadOnlyList<ChainExecutionDto>> GetChainExecutions(
        Guid familyId,
        Guid? chainDefinitionId,
        ChainExecutionStatus? status,
        [Service] IQueryBus queryBus,
        CancellationToken ct)
    {
        var query = new GetChainExecutionsQuery(
            FamilyId.From(familyId),
            chainDefinitionId.HasValue ? ChainDefinitionId.From(chainDefinitionId.Value) : null,
            status);

        var executions = await queryBus.QueryAsync<IReadOnlyList<ChainExecution>>(query, ct);
        return executions.Select(ChainMapper.ToDto).ToList();
    }

    [Authorize]
    public async Task<ChainExecutionDto?> GetChainExecution(
        Guid id,
        [Service] IQueryBus queryBus,
        CancellationToken ct)
    {
        var query = new GetChainExecutionQuery(ChainExecutionId.From(id));
        var execution = await queryBus.QueryAsync<ChainExecution?>(query, ct);
        return execution is null ? null : ChainMapper.ToDto(execution);
    }

    [Authorize]
    public IReadOnlyList<TriggerCatalogEntryDto> GetAvailableTriggers(
        [Service] IChainRegistry registry)
    {
        return registry.GetTriggers().Select(ChainMapper.ToDto).ToList();
    }

    [Authorize]
    public IReadOnlyList<ActionCatalogEntryDto> GetAvailableActions(
        string? compatibleWithTrigger,
        [Service] IChainRegistry registry)
    {
        var actions = compatibleWithTrigger is not null
            ? registry.GetActionsCompatibleWith(compatibleWithTrigger)
            : registry.GetActions();

        return actions.Select(ChainMapper.ToDto).ToList();
    }

    [Authorize]
    public async Task<IReadOnlyList<ChainEntityMappingDto>> GetChainEntityMappings(
        Guid entityId,
        string? entityType,
        [Service] IChainExecutionRepository repository,
        CancellationToken ct)
    {
        var mappings = await repository.GetEntityMappingsAsync(entityId, entityType, ct);
        return mappings.Select(ChainMapper.ToDto).ToList();
    }
}
