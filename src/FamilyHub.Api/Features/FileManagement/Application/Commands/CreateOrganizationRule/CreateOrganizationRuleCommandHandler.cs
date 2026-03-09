using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateOrganizationRule;

public sealed class CreateOrganizationRuleCommandHandler(
    IOrganizationRuleRepository ruleRepository,
    TimeProvider timeProvider)
    : ICommandHandler<CreateOrganizationRuleCommand, CreateOrganizationRuleResult>
{
    public async ValueTask<CreateOrganizationRuleResult> Handle(
        CreateOrganizationRuleCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var maxPriority = await ruleRepository.GetMaxPriorityAsync(command.FamilyId, cancellationToken);

        var rule = OrganizationRule.Create(
            command.Name,
            command.FamilyId,
            command.UserId,
            command.ConditionsJson,
            command.ConditionLogic,
            command.ActionType,
            command.ActionsJson,
            maxPriority + 1,
            utcNow);

        await ruleRepository.AddAsync(rule, cancellationToken);

        return new CreateOrganizationRuleResult(true, rule.Id.Value);
    }
}
