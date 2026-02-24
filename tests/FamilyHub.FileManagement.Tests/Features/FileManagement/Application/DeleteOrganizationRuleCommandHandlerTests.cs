using FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteOrganizationRule;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class DeleteOrganizationRuleCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteRule()
    {
        var ruleRepo = new FakeOrganizationRuleRepository();
        var handler = new DeleteOrganizationRuleCommandHandler(ruleRepo);

        var familyId = FamilyId.New();
        var rule = OrganizationRule.Create(
            "Test", familyId, UserId.New(),
            "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1);
        ruleRepo.Rules.Add(rule);

        var command = new DeleteOrganizationRuleCommand(rule.Id, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        ruleRepo.Rules.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenNotFound()
    {
        var ruleRepo = new FakeOrganizationRuleRepository();
        var handler = new DeleteOrganizationRuleCommandHandler(ruleRepo);

        var command = new DeleteOrganizationRuleCommand(OrganizationRuleId.New(), FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.OrganizationRuleNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenDifferentFamily()
    {
        var ruleRepo = new FakeOrganizationRuleRepository();
        var handler = new DeleteOrganizationRuleCommandHandler(ruleRepo);

        var rule = OrganizationRule.Create(
            "Test", FamilyId.New(), UserId.New(),
            "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1);
        ruleRepo.Rules.Add(rule);

        var command = new DeleteOrganizationRuleCommand(rule.Id, FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
