using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

/// <summary>
/// An auto-organization rule that matches files in the inbox and performs actions.
/// Rules are evaluated in priority order (lower number = higher priority), first match wins.
/// Conditions and actions are stored as JSON for schema flexibility.
/// </summary>
public sealed class OrganizationRule : AggregateRoot<OrganizationRuleId>
{
#pragma warning disable CS8618
    private OrganizationRule() { }
#pragma warning restore CS8618

    public static OrganizationRule Create(
        string name,
        FamilyId familyId,
        UserId createdBy,
        string conditionsJson,
        ConditionLogic conditionLogic,
        RuleActionType actionType,
        string actionsJson,
        int priority,
        DateTimeOffset utcNow)
    {
        return new OrganizationRule
        {
            Id = OrganizationRuleId.New(),
            Name = name,
            FamilyId = familyId,
            CreatedBy = createdBy,
            ConditionsJson = conditionsJson,
            ConditionLogic = conditionLogic,
            ActionType = actionType,
            ActionsJson = actionsJson,
            Priority = priority,
            IsEnabled = true,
            CreatedAt = utcNow.UtcDateTime,
            UpdatedAt = utcNow.UtcDateTime
        };
    }

    public string Name { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public UserId CreatedBy { get; private set; }
    public string ConditionsJson { get; private set; }
    public ConditionLogic ConditionLogic { get; private set; }
    public RuleActionType ActionType { get; private set; }
    public string ActionsJson { get; private set; }
    public int Priority { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Update(
        string name,
        string conditionsJson,
        ConditionLogic conditionLogic,
        RuleActionType actionType,
        string actionsJson,
        DateTimeOffset utcNow)
    {
        Name = name;
        ConditionsJson = conditionsJson;
        ConditionLogic = conditionLogic;
        ActionType = actionType;
        ActionsJson = actionsJson;
        UpdatedAt = utcNow.UtcDateTime;
    }

    public void SetPriority(int priority, DateTimeOffset utcNow)
    {
        Priority = priority;
        UpdatedAt = utcNow.UtcDateTime;
    }

    public void Enable(DateTimeOffset utcNow)
    {
        IsEnabled = true;
        UpdatedAt = utcNow.UtcDateTime;
    }

    public void Disable(DateTimeOffset utcNow)
    {
        IsEnabled = false;
        UpdatedAt = utcNow.UtcDateTime;
    }
}
