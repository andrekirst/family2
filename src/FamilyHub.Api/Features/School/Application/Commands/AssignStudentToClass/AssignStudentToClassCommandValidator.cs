using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.AssignStudentToClass;

public sealed class AssignStudentToClassCommandValidator : AbstractValidator<AssignStudentToClassCommand>, IInputValidator<AssignStudentToClassCommand>
{
    public AssignStudentToClassCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.StudentId)
            .NotNull()
            .WithMessage("Student ID is required");

        RuleFor(x => x.SchoolId)
            .NotNull()
            .WithMessage("School ID is required");

        RuleFor(x => x.SchoolYearId)
            .NotNull()
            .WithMessage("School year ID is required");

        RuleFor(x => x.ClassName)
            .NotNull()
            .WithMessage("Class name is required");
    }
}
