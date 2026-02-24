using System.Text.Json;
using System.Text.RegularExpressions;
using FamilyHub.Api.Features.FileManagement.Application.Services;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Services;

/// <summary>
/// Evaluates organization rules against files using priority-ordered first-match-wins strategy.
/// Conditions are deserialized from JSON and evaluated with AND/OR logic.
/// </summary>
public sealed class OrganizationRuleEngine : IOrganizationRuleEngine
{
    public RuleMatchPreviewDto? EvaluateFile(StoredFile file, List<OrganizationRule> rules)
    {
        // Rules should already be ordered by priority
        foreach (var rule in rules)
        {
            if (!rule.IsEnabled)
                continue;

            if (MatchesRule(file, rule))
            {
                return new RuleMatchPreviewDto(
                    Matched: true,
                    MatchedRuleId: rule.Id.Value,
                    MatchedRuleName: rule.Name,
                    ActionType: rule.ActionType,
                    ActionsJson: rule.ActionsJson);
            }
        }

        return null;
    }

    private static bool MatchesRule(StoredFile file, OrganizationRule rule)
    {
        List<RuleConditionDto> conditions;
        try
        {
            conditions = JsonSerializer.Deserialize<List<RuleConditionDto>>(rule.ConditionsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch
        {
            return false;
        }

        if (conditions.Count == 0)
            return false;

        return rule.ConditionLogic == ConditionLogic.And
            ? conditions.All(c => EvaluateCondition(file, c))
            : conditions.Any(c => EvaluateCondition(file, c));
    }

    private static bool EvaluateCondition(StoredFile file, RuleConditionDto condition)
    {
        return condition.Type switch
        {
            RuleConditionType.FileExtension => EvaluateFileExtension(file, condition.Value),
            RuleConditionType.MimeType => EvaluateMimeType(file, condition.Value),
            RuleConditionType.FileNameRegex => EvaluateFileNameRegex(file, condition.Value),
            RuleConditionType.SizeGreaterThan => EvaluateSizeGreaterThan(file, condition.Value),
            RuleConditionType.SizeLessThan => EvaluateSizeLessThan(file, condition.Value),
            RuleConditionType.UploadDateAfter => EvaluateUploadDateAfter(file, condition.Value),
            RuleConditionType.UploadDateBefore => EvaluateUploadDateBefore(file, condition.Value),
            _ => false
        };
    }

    private static bool EvaluateFileExtension(StoredFile file, string value)
    {
        var fileName = file.Name.Value;
        var extensions = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return extensions.Any(ext =>
            fileName.EndsWith(ext.StartsWith('.') ? ext : $".{ext}", StringComparison.OrdinalIgnoreCase));
    }

    private static bool EvaluateMimeType(StoredFile file, string value)
    {
        var mimeType = file.MimeType.Value;
        var patterns = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return patterns.Any(pattern =>
        {
            if (pattern.EndsWith("/*"))
                return mimeType.StartsWith(pattern[..^2], StringComparison.OrdinalIgnoreCase);
            return string.Equals(mimeType, pattern, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static bool EvaluateFileNameRegex(StoredFile file, string value)
    {
        try
        {
            return Regex.IsMatch(file.Name.Value, value, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
        }
        catch
        {
            return false;
        }
    }

    private static bool EvaluateSizeGreaterThan(StoredFile file, string value)
        => long.TryParse(value, out var threshold) && file.Size.Value > threshold;

    private static bool EvaluateSizeLessThan(StoredFile file, string value)
        => long.TryParse(value, out var threshold) && file.Size.Value < threshold;

    private static bool EvaluateUploadDateAfter(StoredFile file, string value)
        => DateTime.TryParse(value, out var date) && file.CreatedAt > date;

    private static bool EvaluateUploadDateBefore(StoredFile file, string value)
        => DateTime.TryParse(value, out var date) && file.CreatedAt < date;
}
