using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.DeleteSchool;

public sealed class DeleteSchoolCommandValidator : AbstractValidator<DeleteSchoolCommand>, IInputValidator<DeleteSchoolCommand>
{
    public DeleteSchoolCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.SchoolId)
            .NotNull()
            .WithMessage("School ID is required");
    }
}
