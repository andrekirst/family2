using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.CreateSchoolYear;

public sealed class CreateSchoolYearCommandValidator : AbstractValidator<CreateSchoolYearCommand>, IInputValidator<CreateSchoolYearCommand>
{
    public CreateSchoolYearCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.FederalStateId)
            .NotNull()
            .WithMessage("Federal state is required");

        RuleFor(x => x.StartYear)
            .GreaterThan(2000)
            .WithMessage("Start year must be after 2000");

        RuleFor(x => x.EndYear)
            .GreaterThan(x => x.StartYear)
            .WithMessage("End year must be after start year");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");
    }
}
