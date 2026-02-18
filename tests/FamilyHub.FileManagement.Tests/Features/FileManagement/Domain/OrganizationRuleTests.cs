using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain;

public class OrganizationRuleTests
{
    [Fact]
    public void Create_ShouldInitializeAllProperties()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var rule = OrganizationRule.Create(
            "Move photos",
            familyId,
            userId,
            """[{"Type":1,"Value":".jpg,.png"}]""",
            ConditionLogic.And,
            RuleActionType.MoveToFolder,
            """{"DestinationFolderId":"00000000-0000-0000-0000-000000000001"}""",
            1);

        rule.Name.Should().Be("Move photos");
        rule.FamilyId.Should().Be(familyId);
        rule.CreatedBy.Should().Be(userId);
        rule.ConditionLogic.Should().Be(ConditionLogic.And);
        rule.ActionType.Should().Be(RuleActionType.MoveToFolder);
        rule.Priority.Should().Be(1);
        rule.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void Update_ShouldChangeProperties()
    {
        var rule = CreateTestRule();

        rule.Update(
            "Updated name",
            """[{"Type":2,"Value":"image/*"}]""",
            ConditionLogic.Or,
            RuleActionType.ApplyTags,
            """{"TagIds":["00000000-0000-0000-0000-000000000002"]}""");

        rule.Name.Should().Be("Updated name");
        rule.ConditionLogic.Should().Be(ConditionLogic.Or);
        rule.ActionType.Should().Be(RuleActionType.ApplyTags);
    }

    [Fact]
    public void SetPriority_ShouldUpdatePriority()
    {
        var rule = CreateTestRule();

        rule.SetPriority(5);

        rule.Priority.Should().Be(5);
    }

    [Fact]
    public void Enable_ShouldSetIsEnabledTrue()
    {
        var rule = CreateTestRule();
        rule.Disable();
        rule.IsEnabled.Should().BeFalse();

        rule.Enable();

        rule.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void Disable_ShouldSetIsEnabledFalse()
    {
        var rule = CreateTestRule();

        rule.Disable();

        rule.IsEnabled.Should().BeFalse();
    }

    private static OrganizationRule CreateTestRule() =>
        OrganizationRule.Create(
            "Test rule",
            FamilyId.New(),
            UserId.New(),
            """[{"Type":1,"Value":".jpg"}]""",
            ConditionLogic.And,
            RuleActionType.MoveToFolder,
            """{"DestinationFolderId":"00000000-0000-0000-0000-000000000001"}""",
            1);
}
