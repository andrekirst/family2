using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.UnitTests.Features.Family.Domain;

/// <summary>
/// Unit tests for FamilyMember entity.
/// Tests creation with various roles and validates property assignment.
/// </summary>
public class FamilyMemberTests
{
    [Fact]
    public void Create_ShouldCreateMemberWithValidData()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var role = FamilyRole.Member;

        // Act
        var member = FamilyMember.Create(familyId, userId, role);

        // Assert
        member.Should().NotBeNull();
        member.Id.Value.Should().NotBe(Guid.Empty);
        member.FamilyId.Should().Be(familyId);
        member.UserId.Should().Be(userId);
        member.Role.Should().Be(role);
        member.JoinedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        member.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithOwnerRole_ShouldSetCorrectRole()
    {
        // Act
        var member = FamilyMember.Create(FamilyId.New(), UserId.New(), FamilyRole.Owner);

        // Assert
        member.Role.Should().Be(FamilyRole.Owner);
        member.Role.CanInvite().Should().BeTrue();
    }

    [Fact]
    public void Create_WithAdminRole_ShouldSetCorrectRole()
    {
        // Act
        var member = FamilyMember.Create(FamilyId.New(), UserId.New(), FamilyRole.Admin);

        // Assert
        member.Role.Should().Be(FamilyRole.Admin);
        member.Role.CanInvite().Should().BeTrue();
    }

    [Fact]
    public void Create_WithMemberRole_ShouldNotBeAbleToInvite()
    {
        // Act
        var member = FamilyMember.Create(FamilyId.New(), UserId.New(), FamilyRole.Member);

        // Assert
        member.Role.Should().Be(FamilyRole.Member);
        member.Role.CanInvite().Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();

        // Act
        var member1 = FamilyMember.Create(familyId, userId, FamilyRole.Member);
        var member2 = FamilyMember.Create(familyId, userId, FamilyRole.Member);

        // Assert
        member1.Id.Should().NotBe(member2.Id);
    }

    [Fact]
    public void Create_ShouldSetIsActiveToTrue()
    {
        // Act
        var member = FamilyMember.Create(FamilyId.New(), UserId.New(), FamilyRole.Member);

        // Assert
        member.IsActive.Should().BeTrue();
    }
}
