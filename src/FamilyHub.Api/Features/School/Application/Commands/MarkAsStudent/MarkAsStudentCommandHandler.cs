using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.Repositories;

namespace FamilyHub.Api.Features.School.Application.Commands.MarkAsStudent;

public sealed class MarkAsStudentCommandHandler(
    IStudentRepository studentRepository,
    IFamilyMemberRepository familyMemberRepository)
    : ICommandHandler<MarkAsStudentCommand, MarkAsStudentResult>
{
    public async ValueTask<MarkAsStudentResult> Handle(
        MarkAsStudentCommand command,
        CancellationToken cancellationToken)
    {
        // Verify target FamilyMember belongs to the same family (CRITICAL-1: cross-family IDOR)
        var targetMember = (await familyMemberRepository.GetByIdAsync(command.FamilyMemberId, cancellationToken))!;

        if (targetMember.FamilyId != command.FamilyId)
        {
            throw new DomainException("Family member does not belong to this family", DomainErrorCodes.FamilyMemberNotFound);
        }

        // Create student aggregate
        var student = Student.Create(command.FamilyMemberId, command.FamilyId, command.MarkedByUserId);
        await studentRepository.AddAsync(student, cancellationToken);

        return new MarkAsStudentResult(student.Id);
    }
}
