using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.DeleteSchoolYear;

public sealed class DeleteSchoolYearBusinessValidator : AbstractValidator<DeleteSchoolYearCommand>, IBusinessValidator<DeleteSchoolYearCommand>
{
    public DeleteSchoolYearBusinessValidator(
        IClassAssignmentRepository classAssignmentRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
                !await classAssignmentRepository.ExistsBySchoolYearIdAsync(command.SchoolYearId, cancellationToken))
            .WithErrorCode(DomainErrorCodes.SchoolYearInUse)
            .WithMessage(_ => localizer[DomainErrorCodes.SchoolYearInUse].Value);
    }
}
