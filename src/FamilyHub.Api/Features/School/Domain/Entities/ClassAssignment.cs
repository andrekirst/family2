using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Events;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Domain.Entities;

public sealed class ClassAssignment : AggregateRoot<ClassAssignmentId>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private ClassAssignment() { }
#pragma warning restore CS8618

    public static ClassAssignment Create(
        StudentId studentId,
        SchoolId schoolId,
        SchoolYearId schoolYearId,
        ClassName className,
        FamilyId familyId,
        UserId assignedByUserId,
        DateTimeOffset utcNow)
    {
        var assignment = new ClassAssignment
        {
            Id = ClassAssignmentId.New(),
            StudentId = studentId,
            SchoolId = schoolId,
            SchoolYearId = schoolYearId,
            ClassName = className,
            FamilyId = familyId,
            AssignedByUserId = assignedByUserId,
            AssignedAt = utcNow.UtcDateTime
        };

        assignment.RaiseDomainEvent(new StudentAssignedToClassEvent(
            assignment.Id,
            assignment.StudentId,
            assignment.SchoolId,
            assignment.SchoolYearId,
            assignment.ClassName,
            assignment.FamilyId,
            assignment.AssignedByUserId,
            assignment.AssignedAt
        ));

        return assignment;
    }

    public StudentId StudentId { get; private set; }
    public SchoolId SchoolId { get; private set; }
    public SchoolYearId SchoolYearId { get; private set; }
    public ClassName ClassName { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public UserId AssignedByUserId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    public void Update(
        SchoolId schoolId,
        SchoolYearId schoolYearId,
        ClassName className,
        DateTimeOffset utcNow)
    {
        SchoolId = schoolId;
        SchoolYearId = schoolYearId;
        ClassName = className;

        RaiseDomainEvent(new ClassAssignmentUpdatedEvent(
            Id,
            StudentId,
            schoolId,
            schoolYearId,
            className,
            FamilyId
        ));
    }
}
