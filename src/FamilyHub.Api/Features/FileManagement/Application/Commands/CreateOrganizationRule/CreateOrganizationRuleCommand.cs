using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateOrganizationRule;

public sealed record CreateOrganizationRuleCommand(
    string Name,
    FamilyId FamilyId,
    UserId UserId,
    string ConditionsJson,
    ConditionLogic ConditionLogic,
    RuleActionType ActionType,
    string ActionsJson
) : ICommand<CreateOrganizationRuleResult>;
