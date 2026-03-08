using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Commands.MarkAsStudent;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Application;

public class MarkAsStudentAuthValidatorTests
{
    [Fact]
    public async Task Validate_ShouldFail_WhenCallerNotInFamily()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var familyMemberId = FamilyMemberId.New();

        var memberRepo = new FakeFamilyMemberRepository(); // No existing member
        var validator = new MarkAsStudentAuthValidator(memberRepo, new StubStringLocalizer<DomainErrors>());
        var command = new MarkAsStudentCommand(familyMemberId, familyId, userId);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == DomainErrorCodes.UserNotInFamily);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenCallerLacksPermission()
    {
        // Arrange — caller is a Member (not Owner/Admin), so CanManageStudents() is false
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var callerMember = FamilyMember.Create(familyId, userId, FamilyRole.Member);
        var targetMember = FamilyMember.Create(familyId, UserId.New(), FamilyRole.Member);

        var memberRepo = new FakeFamilyMemberRepository(callerMember, [callerMember, targetMember]);
        var validator = new MarkAsStudentAuthValidator(memberRepo, new StubStringLocalizer<DomainErrors>());
        var command = new MarkAsStudentCommand(targetMember.Id, familyId, userId);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenCallerIsOwner()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var callerMember = FamilyMember.Create(familyId, userId, FamilyRole.Owner);
        var targetMember = FamilyMember.Create(familyId, UserId.New(), FamilyRole.Member);

        var memberRepo = new FakeFamilyMemberRepository(callerMember, [callerMember, targetMember]);
        var validator = new MarkAsStudentAuthValidator(memberRepo, new StubStringLocalizer<DomainErrors>());
        var command = new MarkAsStudentCommand(targetMember.Id, familyId, userId);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
