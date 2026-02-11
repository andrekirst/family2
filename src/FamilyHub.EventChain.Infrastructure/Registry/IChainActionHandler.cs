using FamilyHub.EventChain.Infrastructure.Orchestrator;

namespace FamilyHub.EventChain.Infrastructure.Registry;

public interface IChainActionHandler
{
    string ActionType { get; }
    string Version { get; }
    Task<ActionResult> ExecuteAsync(ActionExecutionContext context, CancellationToken ct = default);
    Task CompensateAsync(ActionExecutionContext context, CancellationToken ct = default);
}

public sealed record ActionExecutionContext(
    string InputPayload,
    ChainExecutionContext ChainContext,
    Guid CorrelationId);

public sealed record ActionResult(
    bool Success,
    string? OutputPayload = null,
    string? ErrorMessage = null,
    IReadOnlyList<CreatedEntity>? CreatedEntities = null);

public sealed record CreatedEntity(
    string EntityType,
    Guid EntityId,
    string Module);
