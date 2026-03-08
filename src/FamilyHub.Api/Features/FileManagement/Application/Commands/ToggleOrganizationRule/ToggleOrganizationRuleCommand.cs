using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ToggleOrganizationRule;

public sealed record ToggleOrganizationRuleCommand(
    OrganizationRuleId RuleId,
    bool IsEnabled
) : ICommand<ToggleOrganizationRuleResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
