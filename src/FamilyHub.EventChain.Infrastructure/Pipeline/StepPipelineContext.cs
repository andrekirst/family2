using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Infrastructure.Orchestrator;
using FamilyHub.EventChain.Infrastructure.Registry;

namespace FamilyHub.EventChain.Infrastructure.Pipeline;

public sealed class StepPipelineContext
{
    public required StepExecution StepExecution { get; init; }
    public required ChainExecution ChainExecution { get; init; }
    public required ChainDefinitionStep StepDefinition { get; init; }
    public required ChainExecutionContext ExecutionContext { get; init; }
    public required Guid CorrelationId { get; init; }
    public ActionResult? Result { get; set; }
    public Exception? Exception { get; set; }
    public bool ShouldSkip { get; set; }
}
