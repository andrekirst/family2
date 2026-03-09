using FamilyHub.Api.Features.FileManagement.Application.Queries.GetOrganizationRules;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetOrganizationRulesQueryHandlerTests
{
    private readonly IOrganizationRuleRepository _ruleRepo = Substitute.For<IOrganizationRuleRepository>();
    private readonly GetOrganizationRulesQueryHandler _handler;

    public GetOrganizationRulesQueryHandlerTests()
    {
        _handler = new GetOrganizationRulesQueryHandler(_ruleRepo);
    }

    [Fact]
    public async Task Handle_ShouldReturnRules()
    {
        var familyId = FamilyId.New();
        _ruleRepo.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns([
                OrganizationRule.Create("A", familyId, UserId.New(), "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1),
                OrganizationRule.Create("B", familyId, UserId.New(), "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 2)
            ]);

        var query = new GetOrganizationRulesQuery()
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoRules()
    {
        _ruleRepo.GetByFamilyIdAsync(FamilyId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(new List<OrganizationRule>());

        var query = new GetOrganizationRulesQuery()
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
