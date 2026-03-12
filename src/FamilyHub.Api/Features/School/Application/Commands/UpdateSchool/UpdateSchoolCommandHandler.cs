using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.School.Domain.Repositories;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateSchool;

public sealed class UpdateSchoolCommandHandler(
    ISchoolRepository schoolRepository,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateSchoolCommand, Result<UpdateSchoolResult>>
{
    public async ValueTask<Result<UpdateSchoolResult>> Handle(
        UpdateSchoolCommand command,
        CancellationToken cancellationToken)
    {
        var school = await schoolRepository.GetByIdAsync(command.SchoolId, cancellationToken);
        if (school is null || school.FamilyId != command.FamilyId)
        {
            return DomainError.NotFound(DomainErrorCodes.SchoolNotFound, "School not found");
        }

        var utcNow = timeProvider.GetUtcNow();
        school.Update(command.Name, command.FederalStateId, command.City, command.PostalCode, utcNow);
        await schoolRepository.UpdateAsync(school, cancellationToken);

        return new UpdateSchoolResult(school);
    }
}
