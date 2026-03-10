using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateOrganizationRule;

public sealed class UpdateOrganizationRuleCommandHandler(
    IOrganizationRuleRepository ruleRepository,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateOrganizationRuleCommand, Result<UpdateOrganizationRuleResult>>
{
    public async ValueTask<Result<UpdateOrganizationRuleResult>> Handle(
        UpdateOrganizationRuleCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var rule = await ruleRepository.GetByIdAsync(command.RuleId, cancellationToken);
        if (rule is null)
        {
            return DomainError.NotFound(DomainErrorCodes.OrganizationRuleNotFound, "Organization rule not found");
        }

        if (rule.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Cannot modify rule from another family");
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
