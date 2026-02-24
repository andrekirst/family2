using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Models;

public sealed record RuleMatchPreviewDto(
    bool Matched,
    Guid? MatchedRuleId,
    string? MatchedRuleName,
    RuleActionType? ActionType,
    string? ActionsJson);
