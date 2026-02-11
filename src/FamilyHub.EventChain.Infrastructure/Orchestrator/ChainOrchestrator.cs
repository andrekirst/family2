using System.Text.Json;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Enums;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Infrastructure.Pipeline;
using Microsoft.Extensions.Logging;

namespace FamilyHub.EventChain.Infrastructure.Orchestrator;

public sealed class ChainOrchestrator(
    IChainDefinitionRepository definitionRepository,
    IChainExecutionRepository executionRepository,
    StepPipeline pipeline,
    ILogger<ChainOrchestrator> logger) : IChainOrchestrator
{
    public async Task TryTriggerChainsAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var eventType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name;

        var matchingDefinitions = await definitionRepository.GetEnabledByTriggerEventTypeAsync(eventType, ct);

        if (matchingDefinitions.Count == 0)
        {
            logger.LogDebug("No chain definitions match event {EventType}", eventType);
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

                await executionRepository.AddAsync(execution, ct);
                await executionRepository.SaveChangesAsync(ct);

                // Execute asynchronously (fire and forget for dual dispatch)
                _ = Task.Run(() => ExecuteChainAsync(execution, definition, ct), ct);

                logger.LogInformation(
                    "Chain {ChainName} triggered by {EventType}. ExecutionId={ExecutionId}, CorrelationId={CorrelationId}",
                    definition.Name.Value,
                    eventType,
                    execution.Id.Value,
                    execution.CorrelationId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to trigger chain {ChainName} for event {EventType}",
                    definition.Name.Value, eventType);
            }
        }
    }

    public async Task ExecuteChainAsync(ChainExecution execution, ChainDefinition definition, CancellationToken ct = default)
    {
        try
        {
            execution.MarkRunning();
            await executionRepository.UpdateAsync(execution, ct);
            await executionRepository.SaveChangesAsync(ct);

            var context = new ChainExecutionContext(execution.Context);
            context.SetTriggerData(execution.TriggerPayload);

            var steps = definition.Steps.OrderBy(s => s.StepOrder).ToList();
            var hasFailures = false;

            foreach (var stepDef in steps)
            {
                var stepExecution = execution.StepExecutions
                    .FirstOrDefault(s => s.StepAlias == stepDef.Alias.Value);

                if (stepExecution is null) continue;

                // Evaluate condition
                if (!context.EvaluateCondition(stepDef.ConditionExpression))
                {
                    stepExecution.MarkSkipped();
                    logger.LogInformation(
                        "Step {StepAlias} skipped (condition not met)",
                        stepDef.Alias.Value);
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
                    await pipeline.ExecuteAsync(pipelineContext, ct);

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
                            await executionRepository.AddEntityMappingAsync(mapping, ct);
                        }
                    }
                }
                catch (Exception ex)
                {
                    hasFailures = true;
                    logger.LogError(ex,
                        "Step {StepAlias} in chain {ChainName} failed permanently",
                        stepDef.Alias.Value, definition.Name.Value);
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
                    execution.MarkFailed("All steps failed");
                else
                    execution.MarkPartiallyCompleted();
            }
            else
            {
                execution.MarkCompleted();
            }

            await executionRepository.UpdateAsync(execution, ct);
            await executionRepository.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Chain execution {ExecutionId} failed unexpectedly",
                execution.Id.Value);

            execution.MarkFailed(ex.Message);
            await executionRepository.UpdateAsync(execution, ct);
            await executionRepository.SaveChangesAsync(ct);
        }
    }

    public async Task ResumeStepAsync(Guid stepExecutionId, CancellationToken ct = default)
    {
        var stepExecution = await executionRepository.GetStepExecutionAsync(stepExecutionId, ct);
        if (stepExecution is null) return;

        var execution = await executionRepository.GetByIdWithStepsAsync(stepExecution.ChainExecutionId, ct);
        if (execution is null) return;

        var definition = await definitionRepository.GetByIdWithStepsAsync(execution.ChainDefinitionId, ct);
        if (definition is null) return;

        var stepDef = definition.Steps.FirstOrDefault(s => s.Alias.Value == stepExecution.StepAlias);
        if (stepDef is null) return;

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
            await pipeline.ExecuteAsync(pipelineContext, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Resume of step {StepId} failed", stepExecutionId);
        }

        execution.UpdateContext(context.ToJson());
        await executionRepository.UpdateAsync(execution, ct);
        await executionRepository.SaveChangesAsync(ct);
    }
}
