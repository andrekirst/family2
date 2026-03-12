using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.School.Domain.Repositories;

namespace FamilyHub.Api.Features.School.Application.Commands.DeleteSchool;

public sealed class DeleteSchoolCommandHandler(
    ISchoolRepository schoolRepository)
    : ICommandHandler<DeleteSchoolCommand, Result<bool>>
{
    public async ValueTask<Result<bool>> Handle(
        DeleteSchoolCommand command,
        CancellationToken cancellationToken)
    {
        var school = await schoolRepository.GetByIdAsync(command.SchoolId, cancellationToken);
        if (school is null || school.FamilyId != command.FamilyId)
        {
            return DomainError.NotFound(DomainErrorCodes.SchoolNotFound, "School not found");
        }

        await schoolRepository.DeleteAsync(school, cancellationToken);

        return true;
    }
}
