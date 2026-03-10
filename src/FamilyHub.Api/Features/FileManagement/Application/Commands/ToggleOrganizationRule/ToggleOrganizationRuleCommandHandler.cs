using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ToggleOrganizationRule;

public sealed class ToggleOrganizationRuleCommandHandler(
    IOrganizationRuleRepository ruleRepository,
    TimeProvider timeProvider)
    : ICommandHandler<ToggleOrganizationRuleCommand, Result<ToggleOrganizationRuleResult>>
{
    public async ValueTask<Result<ToggleOrganizationRuleResult>> Handle(
        ToggleOrganizationRuleCommand command,
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

        if (command.IsEnabled)
        {
            rule.Enable(utcNow);
        }
        else
        {
            rule.Disable(utcNow);
        }

        return new ToggleOrganizationRuleResult(true);
    }
}
