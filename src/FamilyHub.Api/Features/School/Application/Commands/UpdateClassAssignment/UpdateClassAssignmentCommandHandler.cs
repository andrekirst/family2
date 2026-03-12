using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.School.Domain.Repositories;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateClassAssignment;

public sealed class UpdateClassAssignmentCommandHandler(
    IClassAssignmentRepository classAssignmentRepository,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateClassAssignmentCommand, Result<UpdateClassAssignmentResult>>
{
    public async ValueTask<Result<UpdateClassAssignmentResult>> Handle(
        UpdateClassAssignmentCommand command,
        CancellationToken cancellationToken)
    {
        var assignment = await classAssignmentRepository.GetByIdAsync(command.ClassAssignmentId, cancellationToken);
        if (assignment is null || assignment.FamilyId != command.FamilyId)
        {
            return DomainError.NotFound(DomainErrorCodes.ClassAssignmentNotFound, "Class assignment not found");
        }

        var utcNow = timeProvider.GetUtcNow();
        assignment.Update(command.SchoolId, command.SchoolYearId, command.ClassName, utcNow);
        await classAssignmentRepository.UpdateAsync(assignment, cancellationToken);

        return new UpdateClassAssignmentResult(assignment);
    }
}
