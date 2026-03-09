using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateOrganizationRule;

public sealed record CreateOrganizationRuleCommand(
    string Name,
    string ConditionsJson,
    ConditionLogic ConditionLogic,
    RuleActionType ActionType,
    string ActionsJson
) : ICommand<Result<CreateOrganizationRuleResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
