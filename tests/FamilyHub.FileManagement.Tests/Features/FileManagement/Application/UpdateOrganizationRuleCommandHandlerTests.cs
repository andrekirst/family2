using FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateOrganizationRule;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class UpdateOrganizationRuleCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateRule()
    {
        var ruleRepo = new FakeOrganizationRuleRepository();
        var handler = new UpdateOrganizationRuleCommandHandler(ruleRepo);

        var familyId = FamilyId.New();
        var rule = OrganizationRule.Create(
            "Old name", familyId, UserId.New(),
            """[{"Type":1,"Value":".jpg"}]""",
            ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1);
        ruleRepo.Rules.Add(rule);

        var command = new UpdateOrganizationRuleCommand(
            rule.Id, "New name",
            """[{"Type":2,"Value":"image/*"}]""",
            ConditionLogic.Or,
            RuleActionType.ApplyTags,
            """{"TagIds":[]}""",
            familyId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        rule.Name.Should().Be("New name");
        rule.ConditionLogic.Should().Be(ConditionLogic.Or);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenNotFound()
    {
        var ruleRepo = new FakeOrganizationRuleRepository();
        var handler = new UpdateOrganizationRuleCommandHandler(ruleRepo);

        var command = new UpdateOrganizationRuleCommand(
            OrganizationRuleId.New(), "Name", "[]",
            ConditionLogic.And, RuleActionType.MoveToFolder, "{}", FamilyId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.OrganizationRuleNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenDifferentFamily()
    {
        var ruleRepo = new FakeOrganizationRuleRepository();
        var handler = new UpdateOrganizationRuleCommandHandler(ruleRepo);

        var rule = OrganizationRule.Create(
            "Name", FamilyId.New(), UserId.New(),
            "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1);
        ruleRepo.Rules.Add(rule);

        var command = new UpdateOrganizationRuleCommand(
            rule.Id, "Name", "[]",
            ConditionLogic.And, RuleActionType.MoveToFolder, "{}", FamilyId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
