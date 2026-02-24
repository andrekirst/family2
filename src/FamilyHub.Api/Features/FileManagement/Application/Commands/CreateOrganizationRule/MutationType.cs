using System.Security.Claims;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateOrganizationRule;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<CreateOrganizationRuleResult> CreateOrganizationRule(
        string name,
        string conditionsJson,
        string conditionLogic,
        string actionType,
        string actionsJson,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var user = await userRepository.GetByExternalIdAsync(
            ExternalUserId.From(externalUserIdString), cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found");

        var familyId = user.FamilyId
            ?? throw new UnauthorizedAccessException("User is not a member of any family");

        var parsedLogic = Enum.Parse<ConditionLogic>(conditionLogic, ignoreCase: true);
        var parsedActionType = Enum.Parse<RuleActionType>(actionType, ignoreCase: true);

        var command = new CreateOrganizationRuleCommand(
            name,
            familyId,
            user.Id,
            conditionsJson,
            parsedLogic,
            parsedActionType,
            actionsJson);

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
