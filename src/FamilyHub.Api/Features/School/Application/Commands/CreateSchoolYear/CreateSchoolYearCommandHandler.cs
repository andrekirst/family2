using FamilyHub.Common.Application;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.Repositories;

namespace FamilyHub.Api.Features.School.Application.Commands.CreateSchoolYear;

public sealed class CreateSchoolYearCommandHandler(
    ISchoolYearRepository schoolYearRepository,
    TimeProvider timeProvider)
    : ICommandHandler<CreateSchoolYearCommand, Result<CreateSchoolYearResult>>
{
    public async ValueTask<Result<CreateSchoolYearResult>> Handle(
        CreateSchoolYearCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var schoolYear = SchoolYear.Create(
            command.FamilyId,
            command.FederalStateId,
            command.StartYear,
            command.EndYear,
            command.StartDate,
            command.EndDate,
            utcNow);

        await schoolYearRepository.AddAsync(schoolYear, cancellationToken);

        return new CreateSchoolYearResult(schoolYear.Id, schoolYear);
    }
}
