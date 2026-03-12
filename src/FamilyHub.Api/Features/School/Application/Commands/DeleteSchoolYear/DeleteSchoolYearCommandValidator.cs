using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.DeleteSchoolYear;

public sealed class DeleteSchoolYearCommandValidator : AbstractValidator<DeleteSchoolYearCommand>, IInputValidator<DeleteSchoolYearCommand>
{
    public DeleteSchoolYearCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.SchoolYearId)
            .NotNull()
            .WithMessage("School year ID is required");
    }
}
