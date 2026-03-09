using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ReorderOrganizationRules;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<object> ReorderOrganizationRules(
        List<Guid> ruleIdsInOrder,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new ReorderOrganizationRulesCommand(
            ruleIdsInOrder);

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Match<object>(
            success => success,
            error => MutationError.FromDomainError(error));
    }
}
