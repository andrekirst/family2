using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.EventChain.Application.Commands.CreateChainDefinition;
using FamilyHub.Api.Features.EventChain.Application.Commands.UpdateChainDefinition;
using FamilyHub.Api.Features.EventChain.Application.Commands.DeleteChainDefinition;
using FamilyHub.Api.Features.EventChain.Application.Commands.EnableChainDefinition;
using FamilyHub.Api.Features.EventChain.Application.Commands.DisableChainDefinition;
using FamilyHub.Api.Features.EventChain.Application.Commands.ExecuteChain;
using FamilyHub.Api.Features.EventChain.Application.Mappers;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Domain.ValueObjects;
using FamilyHub.Api.Features.EventChain.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.EventChain.GraphQL;

[ExtendObjectType(typeof(EventChainMutation))]
public class ChainMutations
{
    [Authorize]
    public async Task<CreateChainDefinitionPayload> CreateChainDefinition(
        CreateChainDefinitionInput input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateChainDefinitionCommand(
                ChainName.From(input.Name),
                input.Description,
                input.TriggerEventType,
                input.Steps.Select(s => new CreateStepCommand(
                    StepAlias.From(s.Alias),
                    s.Name,
                    s.ActionType,
                    ActionVersion.From(s.ActionVersion),
                    s.InputMappings,
                    s.Condition,
                    s.Order)).ToList(),
                input.IsEnabled);

            var result = await commandBus.SendAsync(command, cancellationToken);
            return new CreateChainDefinitionPayload(ChainMapper.ToDto(result.CreatedDefinition));
        }
        catch (Exception ex)
        {
            return new CreateChainDefinitionPayload(null,
                [new UserError(ex.Message, "CREATE_FAILED")]);
        }
    }

    [Authorize]
    public async Task<UpdateChainDefinitionPayload> UpdateChainDefinition(
        Guid id,
        UpdateChainDefinitionInput input,
        [Service] ICommandBus commandBus,
        [Service] IChainExecutionRepository executionRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateChainDefinitionCommand(
                ChainDefinitionId.From(id),
                input.Name is not null ? ChainName.From(input.Name) : null,
                input.Description,
                input.IsEnabled,
                input.Steps?.Select(s => new CreateStepCommand(
                    StepAlias.From(s.Alias),
                    s.Name,
                    s.ActionType,
                    ActionVersion.From(s.ActionVersion),
                    s.InputMappings,
                    s.Condition,
                    s.Order)).ToList());

            var result = await commandBus.SendAsync(command, cancellationToken);
            var count = await executionRepository.GetExecutionCountAsync(result.ChainDefinitionId, cancellationToken);
            var lastExec = await executionRepository.GetLastExecutedAtAsync(result.ChainDefinitionId, cancellationToken);
            return new UpdateChainDefinitionPayload(ChainMapper.ToDto(result.UpdatedDefinition, count, lastExec));
        }
        catch (Exception ex)
        {
            return new UpdateChainDefinitionPayload(null,
                [new UserError(ex.Message, "UPDATE_FAILED")]);
        }
    }

    [Authorize]
    public async Task<DeleteChainDefinitionPayload> DeleteChainDefinition(
        Guid id,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteChainDefinitionCommand(ChainDefinitionId.From(id));
            var result = await commandBus.SendAsync(command, cancellationToken);
            return new DeleteChainDefinitionPayload(result.Success);
        }
        catch (Exception ex)
        {
            return new DeleteChainDefinitionPayload(false,
                [new UserError(ex.Message, "DELETE_FAILED")]);
        }
    }

    [Authorize]
    public async Task<ChainDefinitionDto> EnableChainDefinition(
        Guid id,
        [Service] ICommandBus commandBus,
        [Service] IChainExecutionRepository executionRepository,
        CancellationToken cancellationToken)
    {
        var command = new EnableChainDefinitionCommand(ChainDefinitionId.From(id));
        var result = await commandBus.SendAsync(command, cancellationToken);
        var count = await executionRepository.GetExecutionCountAsync(result.ChainDefinitionId, cancellationToken);
        var lastExec = await executionRepository.GetLastExecutedAtAsync(result.ChainDefinitionId, cancellationToken);
        return ChainMapper.ToDto(result.Definition, count, lastExec);
    }

    [Authorize]
    public async Task<ChainDefinitionDto> DisableChainDefinition(
        Guid id,
        [Service] ICommandBus commandBus,
        [Service] IChainExecutionRepository executionRepository,
        CancellationToken cancellationToken)
    {
        var command = new DisableChainDefinitionCommand(ChainDefinitionId.From(id));
        var result = await commandBus.SendAsync(command, cancellationToken);
        var count = await executionRepository.GetExecutionCountAsync(result.ChainDefinitionId, cancellationToken);
        var lastExec = await executionRepository.GetLastExecutedAtAsync(result.ChainDefinitionId, cancellationToken);
        return ChainMapper.ToDto(result.Definition, count, lastExec);
    }

    [Authorize]
    public async Task<ChainExecutionDto> ExecuteChainManually(
        Guid chainDefinitionId,
        string triggerPayload,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new ExecuteChainCommand(
            ChainDefinitionId.From(chainDefinitionId),
            triggerPayload);

        var result = await commandBus.SendAsync(command, cancellationToken);

        return ChainMapper.ToDto(result.Execution);
    }
}
