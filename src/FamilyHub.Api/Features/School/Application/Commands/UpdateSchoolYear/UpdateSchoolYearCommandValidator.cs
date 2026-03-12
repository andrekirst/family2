using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateSchoolYear;

public sealed class UpdateSchoolYearCommandValidator : AbstractValidator<UpdateSchoolYearCommand>, IInputValidator<UpdateSchoolYearCommand>
{
    public UpdateSchoolYearCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.SchoolYearId)
            .NotNull()
            .WithMessage("School year ID is required");

        RuleFor(x => x.EndYear)
            .GreaterThan(x => x.StartYear)
            .WithMessage("End year must be after start year");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");
    }
}
