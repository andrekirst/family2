using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateOrganizationRule;

public sealed class UpdateOrganizationRuleCommandHandler(
    IOrganizationRuleRepository ruleRepository,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateOrganizationRuleCommand, UpdateOrganizationRuleResult>
{
    public async ValueTask<UpdateOrganizationRuleResult> Handle(
        UpdateOrganizationRuleCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var rule = await ruleRepository.GetByIdAsync(command.RuleId, cancellationToken)
            ?? throw new DomainException("Organization rule not found", DomainErrorCodes.OrganizationRuleNotFound);

        if (rule.FamilyId != command.FamilyId)
        {
            throw new DomainException("Cannot modify rule from another family", DomainErrorCodes.Forbidden);
        }

        rule.Update(
            command.Name,
            command.ConditionsJson,
            command.ConditionLogic,
            command.ActionType,
            command.ActionsJson,
            utcNow);

        return new UpdateOrganizationRuleResult(true);
    }
}
