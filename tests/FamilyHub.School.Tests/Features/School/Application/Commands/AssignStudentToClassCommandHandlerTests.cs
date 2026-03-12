using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Commands.AssignStudentToClass;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.Events;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using SchoolEntity = FamilyHub.Api.Features.School.Domain.Entities.School;

namespace FamilyHub.School.Tests.Features.School.Application.Commands;

public class AssignStudentToClassCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateAssignmentAndReturnResult()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var student = Student.Create(FamilyMemberId.New(), familyId, userId, DateTimeOffset.UtcNow);
        var school = SchoolEntity.Create(SchoolName.From("Test School"), familyId, FederalStateId.New(), "City", "12345", DateTimeOffset.UtcNow);
        var schoolYear = SchoolYear.Create(familyId, FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);

        var classAssignmentRepo = new FakeClassAssignmentRepository();
        var studentRepo = new FakeStudentRepository([student]);
        var schoolRepo = new FakeSchoolRepository([school]);
        var schoolYearRepo = new FakeSchoolYearRepository([schoolYear]);
        var handler = new AssignStudentToClassCommandHandler(classAssignmentRepo, studentRepo, schoolRepo, schoolYearRepo, TimeProvider.System);

        var command = new AssignStudentToClassCommand(student.Id, school.Id, schoolYear.Id, ClassName.From("1a"))
        {
            FamilyId = familyId,
            UserId = userId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ClassAssignmentId.Value.Should().NotBe(Guid.Empty);
        result.Value.CreatedAssignment.StudentId.Should().Be(student.Id);
        result.Value.CreatedAssignment.SchoolId.Should().Be(school.Id);
        result.Value.CreatedAssignment.SchoolYearId.Should().Be(schoolYear.Id);
        result.Value.CreatedAssignment.ClassName.Value.Should().Be("1a");
    }

    [Fact]
    public async Task Handle_ShouldAddAssignmentToRepository()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var student = Student.Create(FamilyMemberId.New(), familyId, userId, DateTimeOffset.UtcNow);
        var school = SchoolEntity.Create(SchoolName.From("Test School"), familyId, FederalStateId.New(), "City", "12345", DateTimeOffset.UtcNow);
        var schoolYear = SchoolYear.Create(familyId, FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);

        var classAssignmentRepo = new FakeClassAssignmentRepository();
        var studentRepo = new FakeStudentRepository([student]);
        var schoolRepo = new FakeSchoolRepository([school]);
        var schoolYearRepo = new FakeSchoolYearRepository([schoolYear]);
        var handler = new AssignStudentToClassCommandHandler(classAssignmentRepo, studentRepo, schoolRepo, schoolYearRepo, TimeProvider.System);

        var command = new AssignStudentToClassCommand(student.Id, school.Id, schoolYear.Id, ClassName.From("2b"))
        {
            FamilyId = familyId,
            UserId = userId
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        classAssignmentRepo.AddedAssignments.Should().HaveCount(1);
        classAssignmentRepo.AddedAssignments[0].FamilyId.Should().Be(familyId);
    }

    [Fact]
    public async Task Handle_ShouldRaiseDomainEvent()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var student = Student.Create(FamilyMemberId.New(), familyId, userId, DateTimeOffset.UtcNow);
        var school = SchoolEntity.Create(SchoolName.From("Test School"), familyId, FederalStateId.New(), "City", "12345", DateTimeOffset.UtcNow);
        var schoolYear = SchoolYear.Create(familyId, FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);

        var classAssignmentRepo = new FakeClassAssignmentRepository();
        var studentRepo = new FakeStudentRepository([student]);
        var schoolRepo = new FakeSchoolRepository([school]);
        var schoolYearRepo = new FakeSchoolYearRepository([schoolYear]);
        var handler = new AssignStudentToClassCommandHandler(classAssignmentRepo, studentRepo, schoolRepo, schoolYearRepo, TimeProvider.System);

        var command = new AssignStudentToClassCommand(student.Id, school.Id, schoolYear.Id, ClassName.From("1a"))
        {
            FamilyId = familyId,
            UserId = userId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.CreatedAssignment.DomainEvents.Should().HaveCount(1);
        result.Value.CreatedAssignment.DomainEvents.First().Should().BeOfType<StudentAssignedToClassEvent>();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenStudentDoesNotExist()
    {
        // Arrange
        var classAssignmentRepo = new FakeClassAssignmentRepository();
        var studentRepo = new FakeStudentRepository(); // Empty
        var schoolRepo = new FakeSchoolRepository();
        var schoolYearRepo = new FakeSchoolYearRepository();
        var handler = new AssignStudentToClassCommandHandler(classAssignmentRepo, studentRepo, schoolRepo, schoolYearRepo, TimeProvider.System);

        var command = new AssignStudentToClassCommand(StudentId.New(), SchoolId.New(), SchoolYearId.New(), ClassName.From("1a"))
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.StudentNotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenStudentBelongsToDifferentFamily()
    {
        // Arrange
        var differentFamilyId = FamilyId.New();
        var student = Student.Create(FamilyMemberId.New(), differentFamilyId, UserId.New(), DateTimeOffset.UtcNow);
        var studentRepo = new FakeStudentRepository([student]);

        var classAssignmentRepo = new FakeClassAssignmentRepository();
        var schoolRepo = new FakeSchoolRepository();
        var schoolYearRepo = new FakeSchoolYearRepository();
        var handler = new AssignStudentToClassCommandHandler(classAssignmentRepo, studentRepo, schoolRepo, schoolYearRepo, TimeProvider.System);

        var command = new AssignStudentToClassCommand(student.Id, SchoolId.New(), SchoolYearId.New(), ClassName.From("1a"))
        {
            FamilyId = FamilyId.New(), // Different family
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.StudentNotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenSchoolDoesNotExist()
    {
        // Arrange
        var familyId = FamilyId.New();
        var student = Student.Create(FamilyMemberId.New(), familyId, UserId.New(), DateTimeOffset.UtcNow);
        var studentRepo = new FakeStudentRepository([student]);

        var classAssignmentRepo = new FakeClassAssignmentRepository();
        var schoolRepo = new FakeSchoolRepository(); // Empty
        var schoolYearRepo = new FakeSchoolYearRepository();
        var handler = new AssignStudentToClassCommandHandler(classAssignmentRepo, studentRepo, schoolRepo, schoolYearRepo, TimeProvider.System);

        var command = new AssignStudentToClassCommand(student.Id, SchoolId.New(), SchoolYearId.New(), ClassName.From("1a"))
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.SchoolNotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenSchoolYearDoesNotExist()
    {
        // Arrange
        var familyId = FamilyId.New();
        var student = Student.Create(FamilyMemberId.New(), familyId, UserId.New(), DateTimeOffset.UtcNow);
        var school = SchoolEntity.Create(SchoolName.From("Test School"), familyId, FederalStateId.New(), "City", "12345", DateTimeOffset.UtcNow);
        var studentRepo = new FakeStudentRepository([student]);
        var schoolRepo = new FakeSchoolRepository([school]);

        var classAssignmentRepo = new FakeClassAssignmentRepository();
        var schoolYearRepo = new FakeSchoolYearRepository(); // Empty
        var handler = new AssignStudentToClassCommandHandler(classAssignmentRepo, studentRepo, schoolRepo, schoolYearRepo, TimeProvider.System);

        var command = new AssignStudentToClassCommand(student.Id, school.Id, SchoolYearId.New(), ClassName.From("1a"))
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Category.Should().Be(DomainErrorCategory.NotFound);
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.SchoolYearNotFound);
    }
}
