using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateSchool;

public sealed class UpdateSchoolCommandValidator : AbstractValidator<UpdateSchoolCommand>, IInputValidator<UpdateSchoolCommand>
{
    public UpdateSchoolCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.SchoolId)
            .NotNull()
            .WithMessage("School ID is required");

        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage("School name is required");

        RuleFor(x => x.FederalStateId)
            .NotNull()
            .WithMessage("Federal state is required");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required");

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithMessage("Postal code is required");
    }
}
