using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Queries.GetStudentClassAssignments;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using NSubstitute;
using SchoolEntity = FamilyHub.Api.Features.School.Domain.Entities.School;

namespace FamilyHub.School.Tests.Features.School.Application.Queries;

public class GetStudentClassAssignmentsQueryHandlerTests
{
    private static TimeProvider CreateFixedTimeProvider(DateTimeOffset fixedTime)
    {
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(fixedTime);
        return timeProvider;
    }

    [Fact]
    public async Task Handle_ShouldReturnAssignmentsWithIsCurrentFlag()
    {
        // Arrange
        var familyId = FamilyId.New();
        var studentId = StudentId.New();
        var school = SchoolEntity.Create(SchoolName.From("Test School"), familyId, FederalStateId.New(), "City", "12345", DateTimeOffset.UtcNow);

        // Current school year: covers "today"
        var currentSchoolYear = SchoolYear.Create(familyId, FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);

        // Past school year: does not cover "today"
        var pastSchoolYear = SchoolYear.Create(familyId, FederalStateId.New(), 2024, 2025, new DateOnly(2024, 8, 1), new DateOnly(2025, 7, 31), DateTimeOffset.UtcNow);

        var currentAssignment = ClassAssignment.Create(studentId, school.Id, currentSchoolYear.Id, ClassName.From("2a"), familyId, UserId.New(), DateTimeOffset.UtcNow);
        var pastAssignment = ClassAssignment.Create(studentId, school.Id, pastSchoolYear.Id, ClassName.From("1a"), familyId, UserId.New(), DateTimeOffset.UtcNow);

        var classAssignmentRepo = new FakeClassAssignmentRepository([currentAssignment, pastAssignment]);
        var schoolRepo = new FakeSchoolRepository([school]);
        var schoolYearRepo = new FakeSchoolYearRepository([currentSchoolYear, pastSchoolYear]);

        // Fixed time: January 15, 2026 (within current school year)
        var timeProvider = CreateFixedTimeProvider(new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero));
        var handler = new GetStudentClassAssignmentsQueryHandler(classAssignmentRepo, schoolRepo, schoolYearRepo, timeProvider);

        var query = new GetStudentClassAssignmentsQuery(studentId) { FamilyId = familyId, UserId = UserId.New() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        var current = result.First(a => a.ClassName == "2a");
        current.IsCurrent.Should().BeTrue();
        current.SchoolName.Should().Be("Test School");

        var past = result.First(a => a.ClassName == "1a");
        past.IsCurrent.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoAssignments()
    {
        // Arrange
        var classAssignmentRepo = new FakeClassAssignmentRepository();
        var schoolRepo = new FakeSchoolRepository();
        var schoolYearRepo = new FakeSchoolYearRepository();
        var handler = new GetStudentClassAssignmentsQueryHandler(classAssignmentRepo, schoolRepo, schoolYearRepo, TimeProvider.System);

        var query = new GetStudentClassAssignmentsQuery(StudentId.New()) { FamilyId = FamilyId.New(), UserId = UserId.New() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var studentId = StudentId.New();
        var school = SchoolEntity.Create(SchoolName.From("Grundschule"), familyId, FederalStateId.New(), "City", "12345", DateTimeOffset.UtcNow);
        var schoolYear = SchoolYear.Create(familyId, FederalStateId.New(), 2025, 2026, new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), DateTimeOffset.UtcNow);
        var assignment = ClassAssignment.Create(studentId, school.Id, schoolYear.Id, ClassName.From("3b"), familyId, userId, DateTimeOffset.UtcNow);

        var classAssignmentRepo = new FakeClassAssignmentRepository([assignment]);
        var schoolRepo = new FakeSchoolRepository([school]);
        var schoolYearRepo = new FakeSchoolYearRepository([schoolYear]);

        var timeProvider = CreateFixedTimeProvider(new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero));
        var handler = new GetStudentClassAssignmentsQueryHandler(classAssignmentRepo, schoolRepo, schoolYearRepo, timeProvider);

        var query = new GetStudentClassAssignmentsQuery(studentId) { FamilyId = familyId, UserId = UserId.New() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.Id.Should().Be(assignment.Id.Value);
        dto.StudentId.Should().Be(studentId.Value);
        dto.SchoolId.Should().Be(school.Id.Value);
        dto.SchoolName.Should().Be("Grundschule");
        dto.SchoolYearId.Should().Be(schoolYear.Id.Value);
        dto.ClassName.Should().Be("3b");
        dto.FamilyId.Should().Be(familyId.Value);
        dto.AssignedByUserId.Should().Be(userId.Value);
        dto.IsCurrent.Should().BeTrue();
    }
}
