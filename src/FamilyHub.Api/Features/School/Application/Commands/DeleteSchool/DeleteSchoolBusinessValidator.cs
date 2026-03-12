using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.DeleteSchool;

public sealed class DeleteSchoolBusinessValidator : AbstractValidator<DeleteSchoolCommand>, IBusinessValidator<DeleteSchoolCommand>
{
    public DeleteSchoolBusinessValidator(
        IClassAssignmentRepository classAssignmentRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
                !await classAssignmentRepository.ExistsBySchoolIdAsync(command.SchoolId, cancellationToken))
            .WithErrorCode(DomainErrorCodes.SchoolInUse)
            .WithMessage(_ => localizer[DomainErrorCodes.SchoolInUse].Value);
    }
}
