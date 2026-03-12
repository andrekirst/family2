using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.Events;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Domain;

public class ClassAssignmentAggregateTests
{
    [Fact]
    public void Create_ShouldCreateClassAssignmentWithValidData()
    {
        // Arrange
        var studentId = StudentId.New();
        var schoolId = SchoolId.New();
        var schoolYearId = SchoolYearId.New();
        var className = ClassName.From("1a");
        var familyId = FamilyId.New();
        var assignedByUserId = UserId.New();

        // Act
        var assignment = ClassAssignment.Create(studentId, schoolId, schoolYearId, className, familyId, assignedByUserId, DateTimeOffset.UtcNow);

        // Assert
        assignment.Should().NotBeNull();
        assignment.Id.Value.Should().NotBe(Guid.Empty);
        assignment.StudentId.Should().Be(studentId);
        assignment.SchoolId.Should().Be(schoolId);
        assignment.SchoolYearId.Should().Be(schoolYearId);
        assignment.ClassName.Should().Be(className);
        assignment.FamilyId.Should().Be(familyId);
        assignment.AssignedByUserId.Should().Be(assignedByUserId);
        assignment.AssignedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldRaiseStudentAssignedToClassEvent()
    {
        // Arrange
        var studentId = StudentId.New();
        var schoolId = SchoolId.New();
        var schoolYearId = SchoolYearId.New();
        var className = ClassName.From("2b");
        var familyId = FamilyId.New();
        var assignedByUserId = UserId.New();

        // Act
        var assignment = ClassAssignment.Create(studentId, schoolId, schoolYearId, className, familyId, assignedByUserId, DateTimeOffset.UtcNow);

        // Assert
        assignment.DomainEvents.Should().HaveCount(1);
        var domainEvent = assignment.DomainEvents.First();
        domainEvent.Should().BeOfType<StudentAssignedToClassEvent>();

        var evt = (StudentAssignedToClassEvent)domainEvent;
        evt.ClassAssignmentId.Should().Be(assignment.Id);
        evt.StudentId.Should().Be(studentId);
        evt.SchoolId.Should().Be(schoolId);
        evt.SchoolYearId.Should().Be(schoolYearId);
        evt.ClassName.Should().Be(className);
        evt.FamilyId.Should().Be(familyId);
        evt.AssignedByUserId.Should().Be(assignedByUserId);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange
        var studentId = StudentId.New();
        var schoolId = SchoolId.New();
        var schoolYearId = SchoolYearId.New();
        var className = ClassName.From("3c");
        var familyId = FamilyId.New();
        var userId = UserId.New();

        // Act
        var a1 = ClassAssignment.Create(studentId, schoolId, schoolYearId, className, familyId, userId, DateTimeOffset.UtcNow);
        var a2 = ClassAssignment.Create(studentId, schoolId, schoolYearId, className, familyId, userId, DateTimeOffset.UtcNow);

        // Assert
        a1.Id.Should().NotBe(a2.Id);
    }

    [Fact]
    public void Update_ShouldRaiseClassAssignmentUpdatedEvent()
    {
        // Arrange
        var assignment = ClassAssignment.Create(
            StudentId.New(), SchoolId.New(), SchoolYearId.New(),
            ClassName.From("1a"), FamilyId.New(), UserId.New(), DateTimeOffset.UtcNow);
        assignment.ClearDomainEvents();

        var newSchoolId = SchoolId.New();
        var newSchoolYearId = SchoolYearId.New();
        var newClassName = ClassName.From("2b");

        // Act
        assignment.Update(newSchoolId, newSchoolYearId, newClassName, DateTimeOffset.UtcNow);

        // Assert
        assignment.SchoolId.Should().Be(newSchoolId);
        assignment.SchoolYearId.Should().Be(newSchoolYearId);
        assignment.ClassName.Should().Be(newClassName);
        assignment.DomainEvents.Should().HaveCount(1);
        var evt = assignment.DomainEvents.First();
        evt.Should().BeOfType<ClassAssignmentUpdatedEvent>();

        var updatedEvt = (ClassAssignmentUpdatedEvent)evt;
        updatedEvt.ClassAssignmentId.Should().Be(assignment.Id);
        updatedEvt.SchoolId.Should().Be(newSchoolId);
        updatedEvt.SchoolYearId.Should().Be(newSchoolYearId);
        updatedEvt.ClassName.Should().Be(newClassName);
    }
}
