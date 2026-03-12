using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.School.Domain.Repositories;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateSchoolYear;

public sealed class UpdateSchoolYearCommandHandler(
    ISchoolYearRepository schoolYearRepository,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateSchoolYearCommand, Result<UpdateSchoolYearResult>>
{
    public async ValueTask<Result<UpdateSchoolYearResult>> Handle(
        UpdateSchoolYearCommand command,
        CancellationToken cancellationToken)
    {
        var schoolYear = await schoolYearRepository.GetByIdAsync(command.SchoolYearId, cancellationToken);
        if (schoolYear is null || schoolYear.FamilyId != command.FamilyId)
        {
            return DomainError.NotFound(DomainErrorCodes.SchoolYearNotFound, "School year not found");
        }

        var utcNow = timeProvider.GetUtcNow();
        schoolYear.Update(command.FederalStateId, command.StartYear, command.EndYear, command.StartDate, command.EndDate, utcNow);
        await schoolYearRepository.UpdateAsync(schoolYear, cancellationToken);

        return new UpdateSchoolYearResult(schoolYear);
    }
}
