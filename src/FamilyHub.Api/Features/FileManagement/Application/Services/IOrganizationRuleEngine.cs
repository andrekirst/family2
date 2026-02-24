using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Models;

namespace FamilyHub.Api.Features.FileManagement.Application.Services;

/// <summary>
/// Evaluates organization rules against files.
/// Returns the first matching rule (priority-ordered, first match wins).
/// </summary>
public interface IOrganizationRuleEngine
{
    RuleMatchPreviewDto? EvaluateFile(StoredFile file, List<OrganizationRule> rules);
}
