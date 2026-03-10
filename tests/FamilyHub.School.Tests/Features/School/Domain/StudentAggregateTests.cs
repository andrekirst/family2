using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.Events;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Domain;

public class StudentAggregateTests
{
    [Fact]
    public void Create_ShouldCreateStudentWithValidData()
    {
        // Arrange
        var familyMemberId = FamilyMemberId.New();
        var familyId = FamilyId.New();
        var markedByUserId = UserId.New();

        // Act
        var student = Student.Create(familyMemberId, familyId, markedByUserId, DateTimeOffset.UtcNow);

        // Assert
        student.Should().NotBeNull();
        student.Id.Value.Should().NotBe(Guid.Empty);
        student.FamilyMemberId.Should().Be(familyMemberId);
        student.FamilyId.Should().Be(familyId);
        student.MarkedByUserId.Should().Be(markedByUserId);
        student.MarkedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldRaiseFamilyMemberMarkedAsStudentEvent()
    {
        // Arrange
        var familyMemberId = FamilyMemberId.New();
        var familyId = FamilyId.New();
        var markedByUserId = UserId.New();

        // Act
        var student = Student.Create(familyMemberId, familyId, markedByUserId, DateTimeOffset.UtcNow);

        // Assert
        student.DomainEvents.Should().HaveCount(1);
        var domainEvent = student.DomainEvents.First();
        domainEvent.Should().BeOfType<FamilyMemberMarkedAsStudentEvent>();

        var evt = (FamilyMemberMarkedAsStudentEvent)domainEvent;
        evt.StudentId.Should().Be(student.Id);
        evt.FamilyMemberId.Should().Be(familyMemberId);
        evt.FamilyId.Should().Be(familyId);
        evt.MarkedByUserId.Should().Be(markedByUserId);
        evt.MarkedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange
        var familyMemberId = FamilyMemberId.New();
        var familyId = FamilyId.New();
        var markedByUserId = UserId.New();

        // Act
        var student1 = Student.Create(familyMemberId, familyId, markedByUserId, DateTimeOffset.UtcNow);
        var student2 = Student.Create(familyMemberId, familyId, markedByUserId, DateTimeOffset.UtcNow);

        // Assert
        student1.Id.Should().NotBe(student2.Id);
    }
}
