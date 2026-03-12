using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.Repositories;

namespace FamilyHub.Api.Features.School.Application.Commands.AssignStudentToClass;

public sealed class AssignStudentToClassCommandHandler(
    IClassAssignmentRepository classAssignmentRepository,
    IStudentRepository studentRepository,
    ISchoolRepository schoolRepository,
    ISchoolYearRepository schoolYearRepository,
    TimeProvider timeProvider)
    : ICommandHandler<AssignStudentToClassCommand, Result<AssignStudentToClassResult>>
{
    public async ValueTask<Result<AssignStudentToClassResult>> Handle(
        AssignStudentToClassCommand command,
        CancellationToken cancellationToken)
    {
        // Verify student exists and belongs to same family
        var student = await studentRepository.GetByIdAsync(command.StudentId, cancellationToken);
        if (student is null || student.FamilyId != command.FamilyId)
        {
            return DomainError.NotFound(DomainErrorCodes.StudentNotFound, "Student not found");
        }

        // Verify school exists and belongs to same family
        var school = await schoolRepository.GetByIdAsync(command.SchoolId, cancellationToken);
        if (school is null || school.FamilyId != command.FamilyId)
        {
            return DomainError.NotFound(DomainErrorCodes.SchoolNotFound, "School not found");
        }

        // Verify school year exists and belongs to same family
        var schoolYear = await schoolYearRepository.GetByIdAsync(command.SchoolYearId, cancellationToken);
        if (schoolYear is null || schoolYear.FamilyId != command.FamilyId)
        {
            return DomainError.NotFound(DomainErrorCodes.SchoolYearNotFound, "School year not found");
        }

        var utcNow = timeProvider.GetUtcNow();
        var assignment = ClassAssignment.Create(
            command.StudentId,
            command.SchoolId,
            command.SchoolYearId,
            command.ClassName,
            command.FamilyId,
            command.UserId,
            utcNow);

        await classAssignmentRepository.AddAsync(assignment, cancellationToken);

        return new AssignStudentToClassResult(assignment.Id, assignment);
    }
}
