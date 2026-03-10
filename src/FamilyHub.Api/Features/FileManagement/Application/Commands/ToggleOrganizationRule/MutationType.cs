using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ToggleOrganizationRule;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<bool> ToggleOrganizationRule(
        Guid ruleId,
        bool isEnabled,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new ToggleOrganizationRuleCommand(
            OrganizationRuleId.From(ruleId),
            isEnabled);

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
