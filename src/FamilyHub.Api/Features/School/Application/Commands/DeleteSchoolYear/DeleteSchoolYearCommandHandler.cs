using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.School.Domain.Repositories;

namespace FamilyHub.Api.Features.School.Application.Commands.DeleteSchoolYear;

public sealed class DeleteSchoolYearCommandHandler(
    ISchoolYearRepository schoolYearRepository,
    IClassAssignmentRepository classAssignmentRepository)
    : ICommandHandler<DeleteSchoolYearCommand, Result<bool>>
{
    public async ValueTask<Result<bool>> Handle(
        DeleteSchoolYearCommand command,
        CancellationToken cancellationToken)
    {
        var schoolYear = await schoolYearRepository.GetByIdAsync(command.SchoolYearId, cancellationToken);
        if (schoolYear is null)
        {
            return DomainError.NotFound(DomainErrorCodes.SchoolYearNotFound, "School year not found");
        }

        if (schoolYear.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "School year does not belong to this family");
        }

        // Delete protection: check if school year is referenced by class assignments
        if (await classAssignmentRepository.ExistsBySchoolYearIdAsync(command.SchoolYearId, cancellationToken))
        {
            return DomainError.BusinessRule(DomainErrorCodes.SchoolYearInUse, "School year cannot be deleted because it is referenced by class assignments");
        }

        await schoolYearRepository.DeleteAsync(schoolYear, cancellationToken);

        return true;
    }
}
