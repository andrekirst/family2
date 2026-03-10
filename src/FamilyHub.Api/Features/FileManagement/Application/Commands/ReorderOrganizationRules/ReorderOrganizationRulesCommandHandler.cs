using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ReorderOrganizationRules;

public sealed class ReorderOrganizationRulesCommandHandler(
    IOrganizationRuleRepository ruleRepository,
    TimeProvider timeProvider)
    : ICommandHandler<ReorderOrganizationRulesCommand, Result<ReorderOrganizationRulesResult>>
{
    public async ValueTask<Result<ReorderOrganizationRulesResult>> Handle(
        ReorderOrganizationRulesCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var rules = await ruleRepository.GetByFamilyIdAsync(command.FamilyId, cancellationToken);

        for (var i = 0; i < command.RuleIdsInOrder.Count; i++)
        {
            var ruleId = OrganizationRuleId.From(command.RuleIdsInOrder[i]);
            var rule = rules.FirstOrDefault(r => r.Id == ruleId);
            if (rule is null)
            {
                return DomainError.NotFound(DomainErrorCodes.OrganizationRuleNotFound, "Organization rule not found");
            }

            rule.SetPriority(i + 1, utcNow);
        }

        return new ReorderOrganizationRulesResult(true);
    }
}
