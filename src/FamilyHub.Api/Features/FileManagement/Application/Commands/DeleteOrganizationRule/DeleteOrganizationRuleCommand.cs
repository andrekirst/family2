using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteOrganizationRule;

public sealed record DeleteOrganizationRuleCommand(
    OrganizationRuleId RuleId,
    FamilyId FamilyId
) : ICommand<DeleteOrganizationRuleResult>;
