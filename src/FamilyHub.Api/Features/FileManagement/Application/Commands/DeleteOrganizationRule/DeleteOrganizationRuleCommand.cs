using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteOrganizationRule;

public sealed record DeleteOrganizationRuleCommand(
    OrganizationRuleId RuleId
) : ICommand<DeleteOrganizationRuleResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
