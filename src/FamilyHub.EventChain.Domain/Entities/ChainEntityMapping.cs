using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.EventChain.Domain.Entities;

public sealed class ChainEntityMapping
{
    public Guid Id { get; private set; }
    public ChainExecutionId ChainExecutionId { get; private set; }
    public string StepAlias { get; private set; }
    public string EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public string Module { get; private set; }
    public DateTime CreatedAt { get; private set; }

#pragma warning disable CS8618
    private ChainEntityMapping() { }
#pragma warning restore CS8618

    public static ChainEntityMapping Create(
        ChainExecutionId chainExecutionId,
        string stepAlias,
        string entityType,
        Guid entityId,
        string module)
    {
        return new ChainEntityMapping
        {
            Id = Guid.NewGuid(),
            ChainExecutionId = chainExecutionId,
            StepAlias = stepAlias,
            EntityType = entityType,
            EntityId = entityId,
            Module = module,
            CreatedAt = DateTime.UtcNow
        };
    }
}
