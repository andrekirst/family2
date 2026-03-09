using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateOrganizationRule;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class CreateOrganizationRuleCommandHandlerTests
{
    private readonly IOrganizationRuleRepository _ruleRepo = Substitute.For<IOrganizationRuleRepository>();
    private readonly CreateOrganizationRuleCommandHandler _handler;

    public CreateOrganizationRuleCommandHandlerTests()
    {
        _handler = new CreateOrganizationRuleCommandHandler(_ruleRepo);
    }

    [Fact]
    public async Task Handle_ShouldCreateRule()
    {
        var familyId = FamilyId.New();
        _ruleRepo.GetMaxPriorityAsync(familyId, Arg.Any<CancellationToken>()).Returns(0);

        var command = new CreateOrganizationRuleCommand(
            "Move photos",
            """[{"Type":1,"Value":".jpg"}]""",
            ConditionLogic.And,
            RuleActionType.MoveToFolder,
            """{"DestinationFolderId":"00000000-0000-0000-0000-000000000001"}""")
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _ruleRepo.Received(1).AddAsync(
            Arg.Is<OrganizationRule>(r => r.Name == "Move photos" && r.Priority == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAutoIncrementPriority()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        _ruleRepo.GetMaxPriorityAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(0, 1);

        await _handler.Handle(new CreateOrganizationRuleCommand(
            "Rule 1",
            """[{"Type":1,"Value":".jpg"}]""",
            ConditionLogic.And,
            RuleActionType.MoveToFolder,
            "{}")
        {
            FamilyId = familyId,
            UserId = userId
        }, CancellationToken.None);

        await _handler.Handle(new CreateOrganizationRuleCommand(
            "Rule 2",
            """[{"Type":1,"Value":".png"}]""",
            ConditionLogic.And,
            RuleActionType.MoveToFolder,
            "{}")
        {
            FamilyId = familyId,
            UserId = userId
        }, CancellationToken.None);

        await _ruleRepo.Received(2).AddAsync(Arg.Any<OrganizationRule>(), Arg.Any<CancellationToken>());
        await _ruleRepo.Received(1).AddAsync(
            Arg.Is<OrganizationRule>(r => r.Priority == 1),
            Arg.Any<CancellationToken>());
        await _ruleRepo.Received(1).AddAsync(
            Arg.Is<OrganizationRule>(r => r.Priority == 2),
            Arg.Any<CancellationToken>());
    }
}
