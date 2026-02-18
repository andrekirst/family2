using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Models;

public sealed record OrganizationRuleDto(
    Guid Id,
    string Name,
    string ConditionsJson,
    ConditionLogic ConditionLogic,
    RuleActionType ActionType,
    string ActionsJson,
    int Priority,
    bool IsEnabled,
    Guid CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt);
