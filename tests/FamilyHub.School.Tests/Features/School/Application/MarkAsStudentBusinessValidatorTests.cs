using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Commands.MarkAsStudent;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using NSubstitute;

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

        var studentRepo = Substitute.For<IStudentRepository>();
        var memberRepo = Substitute.For<IFamilyMemberRepository>();

        studentRepo.ExistsByFamilyMemberIdAsync(targetMember.Id, Arg.Any<CancellationToken>())
            .Returns(true);
        memberRepo.ExistsByIdAsync(targetMember.Id, Arg.Any<CancellationToken>())
            .Returns(true);

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

        var studentRepo = Substitute.For<IStudentRepository>();
        var memberRepo = Substitute.For<IFamilyMemberRepository>();

        studentRepo.ExistsByFamilyMemberIdAsync(familyMemberId, Arg.Any<CancellationToken>())
            .Returns(false);
        memberRepo.ExistsByIdAsync(familyMemberId, Arg.Any<CancellationToken>())
            .Returns(false);

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

        var studentRepo = Substitute.For<IStudentRepository>();
        var memberRepo = Substitute.For<IFamilyMemberRepository>();

        studentRepo.ExistsByFamilyMemberIdAsync(targetMember.Id, Arg.Any<CancellationToken>())
            .Returns(false);
        memberRepo.ExistsByIdAsync(targetMember.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var validator = new MarkAsStudentBusinessValidator(studentRepo, memberRepo, new StubStringLocalizer<DomainErrors>());
        var command = new MarkAsStudentCommand(targetMember.Id) { FamilyId = familyId, UserId = userId };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
