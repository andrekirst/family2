using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteOrganizationRule;

public sealed class DeleteOrganizationRuleCommandHandler(
    IOrganizationRuleRepository ruleRepository)
    : ICommandHandler<DeleteOrganizationRuleCommand, Result<DeleteOrganizationRuleResult>>
{
    public async ValueTask<Result<DeleteOrganizationRuleResult>> Handle(
        DeleteOrganizationRuleCommand command,
        CancellationToken cancellationToken)
    {
        var rule = await ruleRepository.GetByIdAsync(command.RuleId, cancellationToken);
        if (rule is null)
        {
            return DomainError.NotFound(DomainErrorCodes.OrganizationRuleNotFound, "Organization rule not found");
        }

        if (rule.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Cannot delete rule from another family");
        }

        await ruleRepository.RemoveAsync(rule, cancellationToken);

        return new DeleteOrganizationRuleResult(true);
    }
}
