using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateOrganizationRule;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class CreateOrganizationRuleCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateRule()
    {
        var ruleRepo = new FakeOrganizationRuleRepository();
        var handler = new CreateOrganizationRuleCommandHandler(ruleRepo);

        var command = new CreateOrganizationRuleCommand(
            "Move photos",
            FamilyId.New(),
            UserId.New(),
            """[{"Type":1,"Value":".jpg"}]""",
            ConditionLogic.And,
            RuleActionType.MoveToFolder,
            """{"DestinationFolderId":"00000000-0000-0000-0000-000000000001"}""");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        ruleRepo.Rules.Should().HaveCount(1);
        ruleRepo.Rules.First().Name.Should().Be("Move photos");
        ruleRepo.Rules.First().Priority.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldAutoIncrementPriority()
    {
        var ruleRepo = new FakeOrganizationRuleRepository();
        var handler = new CreateOrganizationRuleCommandHandler(ruleRepo);
        var familyId = FamilyId.New();
        var userId = UserId.New();

        await handler.Handle(new CreateOrganizationRuleCommand(
            "Rule 1", familyId, userId,
            """[{"Type":1,"Value":".jpg"}]""",
            ConditionLogic.And, RuleActionType.MoveToFolder, "{}"), CancellationToken.None);

        await handler.Handle(new CreateOrganizationRuleCommand(
            "Rule 2", familyId, userId,
            """[{"Type":1,"Value":".png"}]""",
            ConditionLogic.And, RuleActionType.MoveToFolder, "{}"), CancellationToken.None);

        ruleRepo.Rules.Should().HaveCount(2);
        ruleRepo.Rules[0].Priority.Should().Be(1);
        ruleRepo.Rules[1].Priority.Should().Be(2);
    }
}
