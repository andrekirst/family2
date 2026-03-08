using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Commands.MarkAsStudent;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Application;

public class MarkAsStudentBusinessValidatorTests
{
    [Fact]
    public async Task Validate_ShouldFail_WhenAlreadyStudent()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var targetMember = FamilyMember.Create(familyId, UserId.New(), FamilyRole.Member);

        var existingStudent = Api.Features.School.Domain.Entities.Student.Create(targetMember.Id, familyId, userId);
        var studentRepo = new FakeStudentRepository([existingStudent]);
        var memberRepo = new FakeFamilyMemberRepository(null, [targetMember]);
        var validator = new MarkAsStudentBusinessValidator(studentRepo, memberRepo, new StubStringLocalizer<DomainErrors>());
        var command = new MarkAsStudentCommand(targetMember.Id) { FamilyId = familyId, UserId = userId };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == DomainErrorCodes.FamilyMemberAlreadyStudent);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenFamilyMemberNotFound()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var familyMemberId = FamilyMemberId.New();

        var studentRepo = new FakeStudentRepository();
        var memberRepo = new FakeFamilyMemberRepository(); // No members
        var validator = new MarkAsStudentBusinessValidator(studentRepo, memberRepo, new StubStringLocalizer<DomainErrors>());
        var command = new MarkAsStudentCommand(familyMemberId) { FamilyId = familyId, UserId = userId };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == DomainErrorCodes.FamilyMemberNotFound);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenMemberExistsAndNotAlreadyStudent()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var targetMember = FamilyMember.Create(familyId, UserId.New(), FamilyRole.Member);

        var studentRepo = new FakeStudentRepository();
        var memberRepo = new FakeFamilyMemberRepository(null, [targetMember]);
        var validator = new MarkAsStudentBusinessValidator(studentRepo, memberRepo, new StubStringLocalizer<DomainErrors>());
        var command = new MarkAsStudentCommand(targetMember.Id) { FamilyId = familyId, UserId = userId };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
