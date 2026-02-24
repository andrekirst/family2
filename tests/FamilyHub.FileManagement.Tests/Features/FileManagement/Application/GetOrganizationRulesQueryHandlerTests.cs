using FamilyHub.Api.Features.FileManagement.Application.Queries.GetOrganizationRules;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetOrganizationRulesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnRules()
    {
        var ruleRepo = new FakeOrganizationRuleRepository();
        var handler = new GetOrganizationRulesQueryHandler(ruleRepo);

        var familyId = FamilyId.New();
        ruleRepo.Rules.Add(OrganizationRule.Create("A", familyId, UserId.New(), "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1));
        ruleRepo.Rules.Add(OrganizationRule.Create("B", familyId, UserId.New(), "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 2));

        var query = new GetOrganizationRulesQuery(familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoRules()
    {
        var ruleRepo = new FakeOrganizationRuleRepository();
        var handler = new GetOrganizationRulesQueryHandler(ruleRepo);

        var query = new GetOrganizationRulesQuery(FamilyId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
