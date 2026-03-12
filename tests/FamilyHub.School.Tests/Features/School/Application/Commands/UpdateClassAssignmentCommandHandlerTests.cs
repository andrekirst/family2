using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Commands.UpdateClassAssignment;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Application.Commands;

public class UpdateClassAssignmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateAssignmentAndReturnResult()
    {
        // Arrange
        var familyId = FamilyId.New();
        var assignment = ClassAssignment.Create(
            StudentId.New(), SchoolId.New(), SchoolYearId.New(),
            ClassName.From("1a"), familyId, UserId.New(), DateTimeOffset.UtcNow);
        var repo = new FakeClassAssignmentRepository([assignment]);
        var handler = new UpdateClassAssignmentCommandHandler(repo, TimeProvider.System);

        var newSchoolId = SchoolId.New();
        var newSchoolYearId = SchoolYearId.New();
        var newClassName = ClassName.From("2b");
        var command = new UpdateClassAssignmentCommand(assignment.Id, newSchoolId, newSchoolYearId, newClassName)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UpdatedAssignment.SchoolId.Should().Be(newSchoolId);
        result.Value.UpdatedAssignment.SchoolYearId.Should().Be(newSchoolYearId);
        result.Value.UpdatedAssignment.ClassName.Should().Be(newClassName);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAssignmentDoesNotExist()
    {
        // Arrange
        var repo = new FakeClassAssignmentRepository();
        var handler = new UpdateClassAssignmentCommandHandler(repo, TimeProvider.System);

        var command = new UpdateClassAssignmentCommand(ClassAssignmentId.New(), SchoolId.New(), SchoolYearId.New(), ClassName.From("1a"))
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.ClassAssignmentNotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAssignmentBelongsToDifferentFamily()
    {
        // Arrange
        var assignment = ClassAssignment.Create(
            StudentId.New(), SchoolId.New(), SchoolYearId.New(),
            ClassName.From("1a"), FamilyId.New(), UserId.New(), DateTimeOffset.UtcNow);
        var repo = new FakeClassAssignmentRepository([assignment]);
        var handler = new UpdateClassAssignmentCommandHandler(repo, TimeProvider.System);

        var command = new UpdateClassAssignmentCommand(assignment.Id, SchoolId.New(), SchoolYearId.New(), ClassName.From("2b"))
        {
            FamilyId = FamilyId.New(), // Different family
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
    }
}
