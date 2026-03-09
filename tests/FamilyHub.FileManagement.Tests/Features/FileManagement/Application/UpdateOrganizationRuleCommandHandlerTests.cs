using FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateOrganizationRule;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class UpdateOrganizationRuleCommandHandlerTests
{
    private readonly IOrganizationRuleRepository _ruleRepo = Substitute.For<IOrganizationRuleRepository>();
    private readonly UpdateOrganizationRuleCommandHandler _handler;

    public UpdateOrganizationRuleCommandHandlerTests()
    {
        _handler = new UpdateOrganizationRuleCommandHandler(_ruleRepo);
    }

    [Fact]
    public async Task Handle_ShouldUpdateRule()
    {
        var familyId = FamilyId.New();
        var rule = OrganizationRule.Create(
            "Old name", familyId, UserId.New(),
            """[{"Type":1,"Value":".jpg"}]""",
            ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1);
        _ruleRepo.GetByIdAsync(rule.Id, Arg.Any<CancellationToken>()).Returns(rule);

        var command = new UpdateOrganizationRuleCommand(
            rule.Id,
            "New name",
            """[{"Type":2,"Value":"image/*"}]""",
            ConditionLogic.Or,
            RuleActionType.ApplyTags,
            """{"TagIds":[]}""")
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        rule.Name.Should().Be("New name");
        rule.ConditionLogic.Should().Be(ConditionLogic.Or);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenNotFound()
    {
        _ruleRepo.GetByIdAsync(OrganizationRuleId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((OrganizationRule?)null);

        var command = new UpdateOrganizationRuleCommand(
            OrganizationRuleId.New(), "Name", "[]",
            ConditionLogic.And, RuleActionType.MoveToFolder, "{}")
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.OrganizationRuleNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenDifferentFamily()
    {
        var rule = OrganizationRule.Create(
            "Name", FamilyId.New(), UserId.New(),
            "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1);
        _ruleRepo.GetByIdAsync(rule.Id, Arg.Any<CancellationToken>()).Returns(rule);

        var command = new UpdateOrganizationRuleCommand(
            rule.Id, "Name", "[]",
            ConditionLogic.And, RuleActionType.MoveToFolder, "{}")
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
