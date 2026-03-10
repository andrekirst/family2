using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateOrganizationRule;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<bool> CreateOrganizationRule(
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

        var command = new CreateOrganizationRuleCommand(
            name,
            conditionsJson,
            parsedLogic,
            parsedActionType,
            actionsJson);

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Match(
            success => true,
            error => throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(error.Message)
                    .SetCode(error.ErrorCode)
                    .Build()));
    }
}
