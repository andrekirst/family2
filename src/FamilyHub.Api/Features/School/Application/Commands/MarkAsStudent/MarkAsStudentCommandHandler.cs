using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.Repositories;

namespace FamilyHub.Api.Features.School.Application.Commands.MarkAsStudent;

public sealed class MarkAsStudentCommandHandler(
    IStudentRepository studentRepository,
    IFamilyMemberRepository familyMemberRepository,
    TimeProvider timeProvider)
    : ICommandHandler<MarkAsStudentCommand, Result<MarkAsStudentResult>>
{
    public async ValueTask<Result<MarkAsStudentResult>> Handle(
        MarkAsStudentCommand command,
        CancellationToken cancellationToken)
    {
        // Verify target FamilyMember belongs to the same family (CRITICAL-1: cross-family IDOR)
        var targetMember = (await familyMemberRepository.GetByIdAsync(command.FamilyMemberId, cancellationToken))!;

        if (targetMember.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.FamilyMemberNotFound, "Family member does not belong to this family");
        }

        // Create student aggregate
        var utcNow = timeProvider.GetUtcNow();
        var student = Student.Create(command.FamilyMemberId, command.FamilyId, command.UserId, utcNow);
        await studentRepository.AddAsync(student, cancellationToken);

        return new MarkAsStudentResult(student.Id, student);
    }
}
