using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.School.Domain.Events;
using FamilyHub.Api.Features.School.Domain.Repositories;

namespace FamilyHub.Api.Features.School.Application.Commands.RemoveClassAssignment;

public sealed class RemoveClassAssignmentCommandHandler(
    IClassAssignmentRepository classAssignmentRepository)
    : ICommandHandler<RemoveClassAssignmentCommand, Result<bool>>
{
    public async ValueTask<Result<bool>> Handle(
        RemoveClassAssignmentCommand command,
        CancellationToken cancellationToken)
    {
        var assignment = await classAssignmentRepository.GetByIdAsync(command.ClassAssignmentId, cancellationToken);
        if (assignment is null || assignment.FamilyId != command.FamilyId)
        {
            return DomainError.NotFound(DomainErrorCodes.ClassAssignmentNotFound, "Class assignment not found");
        }

        await classAssignmentRepository.DeleteAsync(assignment, cancellationToken);

        return true;
    }
}
