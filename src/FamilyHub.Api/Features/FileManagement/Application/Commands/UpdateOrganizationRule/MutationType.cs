using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateOrganizationRule;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<UpdateOrganizationRuleResult> UpdateOrganizationRule(
        Guid ruleId,
        string name,
        string conditionsJson,
        string conditionLogic,
        string actionType,
        string actionsJson,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var parsedLogic = Enum.Parse<ConditionLogic>(conditionLogic, ignoreCase: true);
        var parsedActionType = Enum.Parse<RuleActionType>(actionType, ignoreCase: true);

        var command = new UpdateOrganizationRuleCommand(
            OrganizationRuleId.From(ruleId),
            name,
            conditionsJson,
            parsedLogic,
            parsedActionType,
            actionsJson);

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
