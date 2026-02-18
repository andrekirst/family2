using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteOrganizationRule;

public sealed class DeleteOrganizationRuleCommandHandler(
    IOrganizationRuleRepository ruleRepository)
    : ICommandHandler<DeleteOrganizationRuleCommand, DeleteOrganizationRuleResult>
{
    public async ValueTask<DeleteOrganizationRuleResult> Handle(
        DeleteOrganizationRuleCommand command,
        CancellationToken cancellationToken)
    {
        var rule = await ruleRepository.GetByIdAsync(command.RuleId, cancellationToken)
            ?? throw new DomainException("Organization rule not found", DomainErrorCodes.OrganizationRuleNotFound);

        if (rule.FamilyId != command.FamilyId)
            throw new DomainException("Cannot delete rule from another family", DomainErrorCodes.Forbidden);

        await ruleRepository.RemoveAsync(rule, cancellationToken);

        return new DeleteOrganizationRuleResult(true);
    }
}
