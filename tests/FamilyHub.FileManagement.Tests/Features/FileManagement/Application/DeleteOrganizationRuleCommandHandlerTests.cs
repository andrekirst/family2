using FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteOrganizationRule;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class DeleteOrganizationRuleCommandHandlerTests
{
    private readonly IOrganizationRuleRepository _ruleRepo = Substitute.For<IOrganizationRuleRepository>();
    private readonly DeleteOrganizationRuleCommandHandler _handler;

    public DeleteOrganizationRuleCommandHandlerTests()
    {
        _handler = new DeleteOrganizationRuleCommandHandler(_ruleRepo);
    }

    [Fact]
    public async Task Handle_ShouldDeleteRule()
    {
        var familyId = FamilyId.New();
        var rule = OrganizationRule.Create(
            "Test", familyId, UserId.New(),
            "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1, DateTimeOffset.UtcNow);
        _ruleRepo.GetByIdAsync(rule.Id, Arg.Any<CancellationToken>()).Returns(rule);

        var command = new DeleteOrganizationRuleCommand(rule.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _ruleRepo.Received(1).RemoveAsync(rule, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenNotFound()
    {
        _ruleRepo.GetByIdAsync(OrganizationRuleId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((OrganizationRule?)null);

        var command = new DeleteOrganizationRuleCommand(OrganizationRuleId.New())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.OrganizationRuleNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenDifferentFamily()
    {
        var rule = OrganizationRule.Create(
            "Test", FamilyId.New(), UserId.New(),
            "[]", ConditionLogic.And, RuleActionType.MoveToFolder, "{}", 1, DateTimeOffset.UtcNow);
        _ruleRepo.GetByIdAsync(rule.Id, Arg.Any<CancellationToken>()).Returns(rule);

        var command = new DeleteOrganizationRuleCommand(rule.Id)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.Forbidden);
    }
}
