using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateOrganizationRule;

public sealed record UpdateOrganizationRuleCommand(
    OrganizationRuleId RuleId,
    string Name,
    string ConditionsJson,
    ConditionLogic ConditionLogic,
    RuleActionType ActionType,
    string ActionsJson,
    FamilyId FamilyId
) : ICommand<UpdateOrganizationRuleResult>;
