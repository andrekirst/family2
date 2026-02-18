using FamilyHub.Api.Features.FileManagement.Application.Commands.ToggleOrganizationRule;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class ToggleOrganizationRuleCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDisableRule()
    {
        var ruleRepo = new FakeOrganizationRuleRepository();
        var handler = new ToggleOrganizationRuleCommandHandler(ruleRepo);

        var familyId = FamilyId.New();
        var rule = OrganizationRule.Create(
            "Test", familyId, UserId.New(),
            "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1);
        ruleRepo.Rules.Add(rule);

        var command = new ToggleOrganizationRuleCommand(rule.Id, false, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        rule.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldEnableRule()
    {
        var ruleRepo = new FakeOrganizationRuleRepository();
        var handler = new ToggleOrganizationRuleCommandHandler(ruleRepo);

        var familyId = FamilyId.New();
        var rule = OrganizationRule.Create(
            "Test", familyId, UserId.New(),
            "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1);
        rule.Disable();
        ruleRepo.Rules.Add(rule);

        var command = new ToggleOrganizationRuleCommand(rule.Id, true, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        rule.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenDifferentFamily()
    {
        var ruleRepo = new FakeOrganizationRuleRepository();
        var handler = new ToggleOrganizationRuleCommandHandler(ruleRepo);

        var rule = OrganizationRule.Create(
            "Test", FamilyId.New(), UserId.New(),
            "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1);
        ruleRepo.Rules.Add(rule);

        var command = new ToggleOrganizationRuleCommand(rule.Id, false, FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
