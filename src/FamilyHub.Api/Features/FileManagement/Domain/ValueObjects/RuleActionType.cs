namespace FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

/// <summary>
/// Types of actions that can be performed by organization rules.
/// </summary>
public enum RuleActionType
{
    MoveToFolder = 1,
    ApplyTags = 2,
    MoveAndTag = 3
}
