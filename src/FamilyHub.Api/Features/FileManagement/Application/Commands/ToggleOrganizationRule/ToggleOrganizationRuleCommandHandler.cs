using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ToggleOrganizationRule;

public sealed class ToggleOrganizationRuleCommandHandler(
    IOrganizationRuleRepository ruleRepository)
    : ICommandHandler<ToggleOrganizationRuleCommand, ToggleOrganizationRuleResult>
{
    public async ValueTask<ToggleOrganizationRuleResult> Handle(
        ToggleOrganizationRuleCommand command,
        CancellationToken cancellationToken)
    {
        var rule = await ruleRepository.GetByIdAsync(command.RuleId, cancellationToken)
            ?? throw new DomainException("Organization rule not found", DomainErrorCodes.OrganizationRuleNotFound);

        if (rule.FamilyId != command.FamilyId)
            throw new DomainException("Cannot modify rule from another family", DomainErrorCodes.Forbidden);

        if (command.IsEnabled)
            rule.Enable();
        else
            rule.Disable();

        return new ToggleOrganizationRuleResult(true);
    }
}
