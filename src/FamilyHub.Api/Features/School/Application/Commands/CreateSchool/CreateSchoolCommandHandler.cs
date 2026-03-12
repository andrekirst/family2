using FamilyHub.Common.Application;
using FamilyHub.Api.Features.School.Domain.Repositories;

namespace FamilyHub.Api.Features.School.Application.Commands.CreateSchool;

public sealed class CreateSchoolCommandHandler(
    ISchoolRepository schoolRepository,
    TimeProvider timeProvider)
    : ICommandHandler<CreateSchoolCommand, Result<CreateSchoolResult>>
{
    public async ValueTask<Result<CreateSchoolResult>> Handle(
        CreateSchoolCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var school = Domain.Entities.School.Create(
            command.Name,
            command.FamilyId,
            command.FederalStateId,
            command.City,
            command.PostalCode,
            utcNow);

        await schoolRepository.AddAsync(school, cancellationToken);

        return new CreateSchoolResult(school.Id, school);
    }
}
