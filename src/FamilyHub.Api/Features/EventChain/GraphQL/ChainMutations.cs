using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.GraphQL;
using FamilyHub.Api.Features.EventChain.Application.Commands;
using FamilyHub.Api.Features.EventChain.Application.Mappers;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Domain.ValueObjects;
using FamilyHub.Api.Features.EventChain.Models;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace FamilyHub.Api.Features.EventChain.GraphQL;

[ExtendObjectType(typeof(AuthMutations))]
public class ChainMutations
{
    [Authorize]
    public async Task<CreateChainDefinitionPayload> CreateChainDefinition(
        CreateChainDefinitionInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IChainDefinitionRepository definitionRepository,
        [Service] IChainExecutionRepository executionRepository,
        CancellationToken ct)
    {
        try
        {
            var (userId, familyId) = await ResolveUserContext(claimsPrincipal, userRepository, ct);

            var command = new CreateChainDefinitionCommand(
                ChainName.From(input.Name),
                input.Description,
                familyId,
                userId,
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

            var result = await commandBus.SendAsync<CreateChainDefinitionResult>(command, ct);
            var definition = await definitionRepository.GetByIdWithStepsAsync(result.ChainDefinitionId, ct);
            return new CreateChainDefinitionPayload(definition is not null ? ChainMapper.ToDto(definition) : null);
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
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IChainDefinitionRepository definitionRepository,
        [Service] IChainExecutionRepository executionRepository,
        CancellationToken ct)
    {
        try
        {
            var (userId, familyId) = await ResolveUserContext(claimsPrincipal, userRepository, ct);

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

            var result = await commandBus.SendAsync<UpdateChainDefinitionResult>(command, ct);
            var definition = await definitionRepository.GetByIdWithStepsAsync(result.ChainDefinitionId, ct);
            var count = await executionRepository.GetExecutionCountAsync(result.ChainDefinitionId, ct);
            var lastExec = await executionRepository.GetLastExecutedAtAsync(result.ChainDefinitionId, ct);
            return new UpdateChainDefinitionPayload(definition is not null ? ChainMapper.ToDto(definition, count, lastExec) : null);
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
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        CancellationToken ct)
    {
        try
        {
            var (userId, familyId) = await ResolveUserContext(claimsPrincipal, userRepository, ct);

            var command = new DeleteChainDefinitionCommand(ChainDefinitionId.From(id));
            var result = await commandBus.SendAsync<DeleteChainDefinitionResult>(command, ct);
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
        [Service] IChainDefinitionRepository definitionRepository,
        [Service] IChainExecutionRepository executionRepository,
        CancellationToken ct)
    {
        var command = new EnableChainDefinitionCommand(ChainDefinitionId.From(id));
        var defId = await commandBus.SendAsync<ChainDefinitionId>(command, ct);
        var definition = await definitionRepository.GetByIdWithStepsAsync(defId, ct)
            ?? throw new InvalidOperationException("Chain definition not found");
        var count = await executionRepository.GetExecutionCountAsync(defId, ct);
        var lastExec = await executionRepository.GetLastExecutedAtAsync(defId, ct);
        return ChainMapper.ToDto(definition, count, lastExec);
    }

    [Authorize]
    public async Task<ChainDefinitionDto> DisableChainDefinition(
        Guid id,
        [Service] ICommandBus commandBus,
        [Service] IChainDefinitionRepository definitionRepository,
        [Service] IChainExecutionRepository executionRepository,
        CancellationToken ct)
    {
        var command = new DisableChainDefinitionCommand(ChainDefinitionId.From(id));
        var defId = await commandBus.SendAsync<ChainDefinitionId>(command, ct);
        var definition = await definitionRepository.GetByIdWithStepsAsync(defId, ct)
            ?? throw new InvalidOperationException("Chain definition not found");
        var count = await executionRepository.GetExecutionCountAsync(defId, ct);
        var lastExec = await executionRepository.GetLastExecutedAtAsync(defId, ct);
        return ChainMapper.ToDto(definition, count, lastExec);
    }

    [Authorize]
    public async Task<ChainExecutionDto> ExecuteChainManually(
        Guid chainDefinitionId,
        string triggerPayload,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IChainExecutionRepository executionRepository,
        CancellationToken ct)
    {
        var (_, familyId) = await ResolveUserContext(claimsPrincipal, userRepository, ct);

        var command = new ExecuteChainCommand(
            ChainDefinitionId.From(chainDefinitionId),
            familyId,
            triggerPayload);

        var result = await commandBus.SendAsync<ExecuteChainResult>(command, ct);

        // Give execution a moment to start
        await Task.Delay(100, ct);

        var execution = await executionRepository.GetByIdWithStepsAsync(result.ChainExecutionId, ct)
            ?? throw new InvalidOperationException("Chain execution not found");
        return ChainMapper.ToDto(execution);
    }

    private static async Task<(UserId UserId, FamilyId FamilyId)> ResolveUserContext(
        ClaimsPrincipal claimsPrincipal,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var user = await userRepository.GetByExternalIdAsync(externalUserId, ct)
            ?? throw new UnauthorizedAccessException("User not found");

        if (!user.FamilyId.HasValue)
            throw new InvalidOperationException("User is not assigned to a family");

        return (user.Id, user.FamilyId.Value);
    }
}
