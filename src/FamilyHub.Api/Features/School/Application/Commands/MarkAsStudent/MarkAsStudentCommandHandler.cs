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
        // Check if family member is already marked as student
        var alreadyExists = await studentRepository.ExistsByFamilyMemberIdAsync(command.FamilyMemberId, cancellationToken);
        if (alreadyExists)
        {
            throw new DomainException("Family member is already marked as a student", DomainErrorCodes.FamilyMemberAlreadyStudent);
        }

        // Verify caller is a member of this family (defense-in-depth: CRITICAL-2)
        var callerMember = await familyMemberRepository.GetByUserAndFamilyAsync(command.MarkedByUserId, command.FamilyId, cancellationToken);
        if (callerMember is null)
        {
            throw new DomainException("User is not a member of this family", DomainErrorCodes.UserNotInFamily);
        }

        // Authorization check at handler level (defense-in-depth: CRITICAL-2)
        if (!callerMember.Role.CanManageStudents())
        {
            throw new UnauthorizedAccessException("Insufficient permissions to manage students");
        }

        // Verify target FamilyMember exists and belongs to the same family (CRITICAL-1: cross-family IDOR)
        var targetMember = await familyMemberRepository.GetByIdAsync(command.FamilyMemberId, cancellationToken);
        if (targetMember is null)
        {
            throw new DomainException("Family member not found", DomainErrorCodes.FamilyMemberNotFound);
        }

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
