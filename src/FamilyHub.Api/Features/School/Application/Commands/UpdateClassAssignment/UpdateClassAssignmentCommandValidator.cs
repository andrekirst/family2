using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateClassAssignment;

public sealed class UpdateClassAssignmentCommandValidator : AbstractValidator<UpdateClassAssignmentCommand>, IInputValidator<UpdateClassAssignmentCommand>
{
    public UpdateClassAssignmentCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.ClassAssignmentId)
            .NotNull()
            .WithMessage("Class assignment ID is required");

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
