using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.EventChain.Domain.Entities;

public sealed class ChainDefinitionStep
{
    public Guid Id { get; private set; }
    public ChainDefinitionId ChainDefinitionId { get; private set; }
    public StepAlias Alias { get; private set; }
    public string Name { get; private set; }
    public string ActionType { get; private set; }
    public ActionVersion ActionVersion { get; private set; }
    public string Module { get; private set; }
    public string InputMappings { get; private set; }
    public string? ConditionExpression { get; private set; }
    public bool IsCompensatable { get; private set; }
    public string? CompensationActionType { get; private set; }
    public int StepOrder { get; private set; }

#pragma warning disable CS8618
    private ChainDefinitionStep() { }
#pragma warning restore CS8618

    public static ChainDefinitionStep Create(
        ChainDefinitionId chainDefinitionId,
        StepAlias alias,
        string name,
        string actionType,
        ActionVersion actionVersion,
        string module,
        string inputMappings,
        string? conditionExpression,
        bool isCompensatable,
        string? compensationActionType,
        int stepOrder)
    {
        return new ChainDefinitionStep
        {
            Id = Guid.NewGuid(),
            ChainDefinitionId = chainDefinitionId,
            Alias = alias,
            Name = name,
            ActionType = actionType,
            ActionVersion = actionVersion,
            Module = module,
            InputMappings = inputMappings,
            ConditionExpression = conditionExpression,
            IsCompensatable = isCompensatable,
            CompensationActionType = compensationActionType,
            StepOrder = stepOrder
        };
    }
}
