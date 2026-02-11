using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Infrastructure.Registry;
using FamilyHub.Api.Features.EventChain.Models;

namespace FamilyHub.Api.Features.EventChain.Application.Mappers;

public static class ChainMapper
{
    public static ChainDefinitionDto ToDto(ChainDefinition definition, int executionCount = 0, DateTime? lastExecutedAt = null)
    {
        return new ChainDefinitionDto(
            Id: definition.Id.Value,
            FamilyId: definition.FamilyId.Value,
            Name: definition.Name.Value,
            Description: definition.Description,
            IsEnabled: definition.IsEnabled,
            IsTemplate: definition.IsTemplate,
            TemplateName: definition.TemplateName,
            Trigger: new TriggerDefinitionDto(
                EventType: definition.TriggerEventType,
                Module: definition.TriggerModule,
                Description: definition.TriggerDescription ?? "",
                OutputSchema: definition.TriggerOutputSchema ?? "{}"),
            Steps: definition.Steps.OrderBy(s => s.StepOrder).Select(ToDto).ToList(),
            CreatedByUserId: definition.CreatedByUserId.Value,
            CreatedAt: definition.CreatedAt,
            UpdatedAt: definition.UpdatedAt,
            Version: definition.Version,
            ExecutionCount: executionCount,
            LastExecutedAt: lastExecutedAt);
    }

    public static StepDefinitionDto ToDto(ChainDefinitionStep step)
    {
        return new StepDefinitionDto(
            Alias: step.Alias.Value,
            Name: step.Name,
            ActionType: step.ActionType,
            ActionVersion: step.ActionVersion.Value,
            Module: step.Module,
            InputMappings: step.InputMappings,
            Condition: step.ConditionExpression,
            IsCompensatable: step.IsCompensatable,
            CompensationActionType: step.CompensationActionType,
            Order: step.StepOrder);
    }

    public static ChainExecutionDto ToDto(ChainExecution execution)
    {
        return new ChainExecutionDto(
            Id: execution.Id.Value,
            ChainDefinitionId: execution.ChainDefinitionId.Value,
            FamilyId: execution.FamilyId.Value,
            CorrelationId: execution.CorrelationId,
            Status: execution.Status.ToString(),
            TriggerEventType: execution.TriggerEventType,
            TriggerPayload: execution.TriggerPayload,
            Context: execution.Context,
            StartedAt: execution.StartedAt,
            CompletedAt: execution.CompletedAt,
            FailedAt: execution.FailedAt,
            ErrorMessage: execution.ErrorMessage,
            StepExecutions: execution.StepExecutions.OrderBy(s => s.StepOrder).Select(ToDto).ToList());
    }

    public static StepExecutionDto ToDto(StepExecution step)
    {
        return new StepExecutionDto(
            Id: step.Id,
            ChainExecutionId: step.ChainExecutionId.Value,
            StepAlias: step.StepAlias,
            StepName: step.StepName,
            ActionType: step.ActionType,
            Status: step.Status.ToString(),
            InputPayload: step.InputPayload,
            OutputPayload: step.OutputPayload,
            ErrorMessage: step.ErrorMessage,
            RetryCount: step.RetryCount,
            StartedAt: step.StartedAt,
            CompletedAt: step.CompletedAt,
            CompensatedAt: step.CompensatedAt);
    }

    public static ChainEntityMappingDto ToDto(ChainEntityMapping mapping)
    {
        return new ChainEntityMappingDto(
            Id: mapping.Id,
            ChainExecutionId: mapping.ChainExecutionId.Value,
            StepAlias: mapping.StepAlias,
            EntityType: mapping.EntityType,
            EntityId: mapping.EntityId,
            Module: mapping.Module,
            CreatedAt: mapping.CreatedAt);
    }

    public static ActionCatalogEntryDto ToDto(ActionDescriptor action)
    {
        return new ActionCatalogEntryDto(
            ActionType: action.ActionType,
            Module: action.Module,
            Name: action.Name,
            Description: action.Description,
            Version: action.Version,
            InputSchema: action.InputSchema,
            OutputSchema: action.OutputSchema,
            IsCompensatable: action.IsCompensatable,
            IsDeprecated: action.IsDeprecated);
    }

    public static TriggerCatalogEntryDto ToDto(TriggerDescriptor trigger)
    {
        return new TriggerCatalogEntryDto(
            EventType: trigger.EventType,
            Module: trigger.Module,
            Name: trigger.Name,
            Description: trigger.Description,
            OutputSchema: trigger.OutputSchema);
    }
}
