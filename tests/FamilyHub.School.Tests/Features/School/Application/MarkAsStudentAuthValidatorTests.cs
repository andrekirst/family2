using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Commands.MarkAsStudent;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using NSubstitute;

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

        var memberRepo = Substitute.For<IFamilyMemberRepository>();
        memberRepo.GetByUserAndFamilyAsync(userId, familyId, Arg.Any<CancellationToken>())
            .Returns((FamilyMember?)null);

        var validator = new MarkAsStudentAuthValidator(memberRepo, new StubStringLocalizer<DomainErrors>());
        var command = new MarkAsStudentCommand(familyMemberId) { FamilyId = familyId, UserId = userId };

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
        var callerMember = FamilyMember.Create(familyId, userId, FamilyRole.Member, DateTimeOffset.UtcNow);
        var targetMember = FamilyMember.Create(familyId, UserId.New(), FamilyRole.Member, DateTimeOffset.UtcNow);

        var memberRepo = Substitute.For<IFamilyMemberRepository>();
        memberRepo.GetByUserAndFamilyAsync(userId, familyId, Arg.Any<CancellationToken>())
            .Returns(callerMember);

        var validator = new MarkAsStudentAuthValidator(memberRepo, new StubStringLocalizer<DomainErrors>());
        var command = new MarkAsStudentCommand(targetMember.Id) { FamilyId = familyId, UserId = userId };

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
        var callerMember = FamilyMember.Create(familyId, userId, FamilyRole.Owner, DateTimeOffset.UtcNow);
        var targetMember = FamilyMember.Create(familyId, UserId.New(), FamilyRole.Member, DateTimeOffset.UtcNow);

        var memberRepo = Substitute.For<IFamilyMemberRepository>();
        memberRepo.GetByUserAndFamilyAsync(userId, familyId, Arg.Any<CancellationToken>())
            .Returns(callerMember);

        var validator = new MarkAsStudentAuthValidator(memberRepo, new StubStringLocalizer<DomainErrors>());
        var command = new MarkAsStudentCommand(targetMember.Id) { FamilyId = familyId, UserId = userId };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
