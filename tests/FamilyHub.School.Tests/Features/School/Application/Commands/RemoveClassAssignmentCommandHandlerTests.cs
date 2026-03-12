using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Commands.RemoveClassAssignment;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Application.Commands;

public class RemoveClassAssignmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRemoveAssignmentAndReturnTrue()
    {
        // Arrange
        var familyId = FamilyId.New();
        var assignment = ClassAssignment.Create(
            StudentId.New(), SchoolId.New(), SchoolYearId.New(),
            ClassName.From("1a"), familyId, UserId.New(), DateTimeOffset.UtcNow);
        var repo = new FakeClassAssignmentRepository([assignment]);
        var handler = new RemoveClassAssignmentCommandHandler(repo);

        var command = new RemoveClassAssignmentCommand(assignment.Id) { FamilyId = familyId, UserId = UserId.New() };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        repo.DeletedAssignments.Should().HaveCount(1);
        repo.DeletedAssignments[0].Id.Should().Be(assignment.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAssignmentDoesNotExist()
    {
        // Arrange
        var repo = new FakeClassAssignmentRepository();
        var handler = new RemoveClassAssignmentCommandHandler(repo);

        var command = new RemoveClassAssignmentCommand(ClassAssignmentId.New()) { FamilyId = FamilyId.New(), UserId = UserId.New() };

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
        var handler = new RemoveClassAssignmentCommandHandler(repo);

        var command = new RemoveClassAssignmentCommand(assignment.Id) { FamilyId = FamilyId.New(), UserId = UserId.New() };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
    }
}
