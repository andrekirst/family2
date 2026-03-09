using FamilyHub.Api.Features.FileManagement.Application.Commands.ReorderOrganizationRules;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class ReorderOrganizationRulesCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReorderRules()
    {
        var ruleRepo = Substitute.For<IOrganizationRuleRepository>();
        var handler = new ReorderOrganizationRulesCommandHandler(ruleRepo);

        var familyId = FamilyId.New();
        var userId = UserId.New();
        var rule1 = OrganizationRule.Create("A", familyId, userId, "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1);
        var rule2 = OrganizationRule.Create("B", familyId, userId, "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 2);
        var rule3 = OrganizationRule.Create("C", familyId, userId, "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 3);

        ruleRepo.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(new List<OrganizationRule> { rule1, rule2, rule3 });

        // Reverse order: C, B, A
        var command = new ReorderOrganizationRulesCommand(
            [rule3.Id.Value, rule2.Id.Value, rule1.Id.Value])
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        rule3.Priority.Should().Be(1);
        rule2.Priority.Should().Be(2);
        rule1.Priority.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenRuleNotFound()
    {
        var ruleRepo = Substitute.For<IOrganizationRuleRepository>();
        var handler = new ReorderOrganizationRulesCommandHandler(ruleRepo);

        var familyId = FamilyId.New();
        ruleRepo.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(new List<OrganizationRule>());

        var command = new ReorderOrganizationRulesCommand(
            [Guid.NewGuid()])
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.OrganizationRuleNotFound);
    }
}
