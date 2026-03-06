using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Events;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Domain.Entities;

public sealed class Student : AggregateRoot<StudentId>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private Student() { }
#pragma warning restore CS8618

    public static Student Create(FamilyMemberId familyMemberId, FamilyId familyId, UserId markedByUserId)
    {
        var student = new Student
        {
            Id = StudentId.New(),
            FamilyMemberId = familyMemberId,
            FamilyId = familyId,
            MarkedByUserId = markedByUserId,
            MarkedAt = DateTime.UtcNow
        };

        student.RaiseDomainEvent(new FamilyMemberMarkedAsStudentEvent(
            student.Id,
            student.FamilyMemberId,
            student.FamilyId,
            student.MarkedByUserId,
            student.MarkedAt
        ));

        return student;
    }

    public FamilyMemberId FamilyMemberId { get; private set; }

    public FamilyId FamilyId { get; private set; }

    public UserId MarkedByUserId { get; private set; }

    public DateTime MarkedAt { get; private set; }
}
