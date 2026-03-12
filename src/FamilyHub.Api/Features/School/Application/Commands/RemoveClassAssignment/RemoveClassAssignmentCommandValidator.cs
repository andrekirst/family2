using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.RemoveClassAssignment;

public sealed class RemoveClassAssignmentCommandValidator : AbstractValidator<RemoveClassAssignmentCommand>, IInputValidator<RemoveClassAssignmentCommand>
{
    public RemoveClassAssignmentCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.ClassAssignmentId)
            .NotNull()
            .WithMessage("Class assignment ID is required");
    }
}
