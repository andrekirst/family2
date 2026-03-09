using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ReorderOrganizationRules;

public sealed class ReorderOrganizationRulesCommandHandler(
    IOrganizationRuleRepository ruleRepository,
    TimeProvider timeProvider)
    : ICommandHandler<ReorderOrganizationRulesCommand, ReorderOrganizationRulesResult>
{
    public async ValueTask<ReorderOrganizationRulesResult> Handle(
        ReorderOrganizationRulesCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var rules = await ruleRepository.GetByFamilyIdAsync(command.FamilyId, cancellationToken);

        for (var i = 0; i < command.RuleIdsInOrder.Count; i++)
        {
            var ruleId = OrganizationRuleId.From(command.RuleIdsInOrder[i]);
            var rule = rules.FirstOrDefault(r => r.Id == ruleId)
                ?? throw new DomainException("Organization rule not found", DomainErrorCodes.OrganizationRuleNotFound);

            rule.SetPriority(i + 1, utcNow);
        }

        return new ReorderOrganizationRulesResult(true);
    }
}
