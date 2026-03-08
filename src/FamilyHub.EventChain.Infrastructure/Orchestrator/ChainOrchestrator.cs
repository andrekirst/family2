using System.Text.Json;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Enums;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Infrastructure.Pipeline;
using Microsoft.Extensions.Logging;

namespace FamilyHub.EventChain.Infrastructure.Orchestrator;

public sealed partial class ChainOrchestrator(
    IChainDefinitionRepository definitionRepository,
    IChainExecutionRepository executionRepository,
    IUnitOfWork unitOfWork,
    StepPipeline pipeline,
    ILogger<ChainOrchestrator> logger) : IChainOrchestrator
{
    public async Task TryTriggerChainsAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name;

        var matchingDefinitions = await definitionRepository.GetEnabledByTriggerEventTypeAsync(eventType, cancellationToken);

        if (matchingDefinitions.Count == 0)
        {
            LogNoChainDefinitionsMatchEventEventtype(logger, eventType);
            return;
        }

        var triggerPayload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

        foreach (var definition in matchingDefinitions)
        {
            try
            {
                var execution = ChainExecution.Start(
                    definition.Id,
                    definition.FamilyId,
                    eventType,
                    domainEvent.EventId,
                    triggerPayload);

                // Create step executions for each definition step
                foreach (var step in definition.Steps.OrderBy(s => s.StepOrder))
                {
                    var stepExecution = StepExecution.Create(
                        execution.Id,
                        step.Alias.Value,
                        step.Name,
                        step.ActionType,
                        step.StepOrder);

                    execution.AddStepExecution(stepExecution);
                }

                await executionRepository.AddAsync(execution, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                // Execute asynchronously (fire and forget for dual dispatch)
                _ = Task.Run(() => ExecuteChainAsync(execution, definition, cancellationToken), cancellationToken);

                LogChainChainnameTriggeredByEventtypeExecutionidExecutionidCorrelationidCorrelationid(logger, definition.Name.Value, eventType, execution.Id.Value, execution.CorrelationId);
            }
            catch (Exception)
            {
                LogFailedToTriggerChainChainnameForEventEventtype(logger, definition.Name.Value, eventType);
            }
        }
    }

    public async Task ExecuteChainAsync(ChainExecution execution, ChainDefinition definition, CancellationToken cancellationToken = default)
    {
        try
        {
            execution.MarkRunning();
            await executionRepository.UpdateAsync(execution, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var context = new ChainExecutionContext(execution.Context);
            context.SetTriggerData(execution.TriggerPayload);

            var steps = definition.Steps.OrderBy(s => s.StepOrder).ToList();
            var hasFailures = false;

            foreach (var stepDef in steps)
            {
                var stepExecution = execution.StepExecutions
                    .FirstOrDefault(s => s.StepAlias == stepDef.Alias.Value);

                if (stepExecution is null)
                {
                    continue;
                }

                // Evaluate condition
                if (!context.EvaluateCondition(stepDef.ConditionExpression))
                {
                    stepExecution.MarkSkipped();
                    LogStepStepaliasSkippedConditionNotMet(logger, stepDef.Alias.Value);
                    continue;
                }

                // Resolve input mappings from context
                var resolvedInput = context.ResolveInputMappings(stepDef.InputMappings);
                stepExecution.SetInputPayload(resolvedInput);

                var pipelineContext = new StepPipelineContext
                {
                    StepExecution = stepExecution,
                    ChainExecution = execution,
                    StepDefinition = stepDef,
                    ExecutionContext = context,
                    CorrelationId = execution.CorrelationId
                };

                try
                {
                    await pipeline.ExecuteAsync(pipelineContext, cancellationToken);

                    // Store entity mappings if any
                    if (pipelineContext.Result?.CreatedEntities is { Count: > 0 } entities)
                    {
                        foreach (var entity in entities)
                        {
                            var mapping = ChainEntityMapping.Create(
                                execution.Id,
                                stepDef.Alias.Value,
                                entity.EntityType,
                                entity.EntityId,
                                entity.Module);
                            await executionRepository.AddEntityMappingAsync(mapping, cancellationToken);
                        }
                    }
                }
                catch (Exception)
                {
                    hasFailures = true;
                    LogStepStepaliasInChainChainnameFailedPermanently(logger, stepDef.Alias.Value, definition.Name.Value);
                    // Continue to next step (partial completion)
                }

                execution.AdvanceStep();
            }

            // Update context on execution
            execution.UpdateContext(context.ToJson());

            if (hasFailures)
            {
                var allFailed = execution.StepExecutions
                    .All(s => s.Status is StepExecutionStatus.Failed or StepExecutionStatus.Skipped);

                if (allFailed)
                {
                    execution.MarkFailed("All steps failed");
                }
                else
                {
                    execution.MarkPartiallyCompleted();
                }
            }
            else
            {
                execution.MarkCompleted();
            }

            await executionRepository.UpdateAsync(execution, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            LogChainExecutionExecutionidFailedUnexpectedly(logger, execution.Id.Value);

            execution.MarkFailed(ex.Message);
            await executionRepository.UpdateAsync(execution, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ResumeStepAsync(Guid stepExecutionId, CancellationToken cancellationToken = default)
    {
        var stepExecution = await executionRepository.GetStepExecutionAsync(stepExecutionId, cancellationToken);
        if (stepExecution is null)
        {
            return;
        }

        var execution = await executionRepository.GetByIdWithStepsAsync(stepExecution.ChainExecutionId, cancellationToken);
        if (execution is null)
        {
            return;
        }

        var definition = await definitionRepository.GetByIdWithStepsAsync(execution.ChainDefinitionId, cancellationToken);
        if (definition is null)
        {
            return;
        }

        var stepDef = definition.Steps.FirstOrDefault(s => s.Alias.Value == stepExecution.StepAlias);
        if (stepDef is null)
        {
            return;
        }

        var context = new ChainExecutionContext(execution.Context);

        var resolvedInput = context.ResolveInputMappings(stepDef.InputMappings);
        stepExecution.SetInputPayload(resolvedInput);

        var pipelineContext = new StepPipelineContext
        {
            StepExecution = stepExecution,
            ChainExecution = execution,
            StepDefinition = stepDef,
            ExecutionContext = context,
            CorrelationId = execution.CorrelationId
        };

        try
        {
            await pipeline.ExecuteAsync(pipelineContext, cancellationToken);
        }
        catch (Exception)
        {
            LogResumeOfStepStepidFailed(logger, stepExecutionId);
        }

        execution.UpdateContext(context.ToJson());
        await executionRepository.UpdateAsync(execution, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    [LoggerMessage(LogLevel.Debug, "No chain definitions match event {eventType}")]
    static partial void LogNoChainDefinitionsMatchEventEventtype(ILogger<ChainOrchestrator> logger, string eventType);

    [LoggerMessage(LogLevel.Information, "Chain {chainName} triggered by {eventType}. ExecutionId={executionId}, CorrelationId={correlationId}")]
    static partial void LogChainChainnameTriggeredByEventtypeExecutionidExecutionidCorrelationidCorrelationid(ILogger<ChainOrchestrator> logger, string chainName, string eventType, Guid executionId, Guid correlationId);

    [LoggerMessage(LogLevel.Error, "Failed to trigger chain {chainName} for event {eventType}")]
    static partial void LogFailedToTriggerChainChainnameForEventEventtype(ILogger<ChainOrchestrator> logger, string chainName, string eventType);

    [LoggerMessage(LogLevel.Information, "Step {stepAlias} skipped (condition not met)")]
    static partial void LogStepStepaliasSkippedConditionNotMet(ILogger<ChainOrchestrator> logger, string stepAlias);

    [LoggerMessage(LogLevel.Error, "Step {stepAlias} in chain {chainName} failed permanently")]
    static partial void LogStepStepaliasInChainChainnameFailedPermanently(ILogger<ChainOrchestrator> logger, string stepAlias, string chainName);

    [LoggerMessage(LogLevel.Error, "Chain execution {executionId} failed unexpectedly")]
    static partial void LogChainExecutionExecutionidFailedUnexpectedly(ILogger<ChainOrchestrator> logger, Guid executionId);

    [LoggerMessage(LogLevel.Error, "Resume of step {stepId} failed")]
    static partial void LogResumeOfStepStepidFailed(ILogger<ChainOrchestrator> logger, Guid stepId);
}
