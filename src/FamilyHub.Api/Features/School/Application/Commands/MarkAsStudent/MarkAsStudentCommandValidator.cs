using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.School.Application.Commands.MarkAsStudent;

public sealed class MarkAsStudentCommandValidator : AbstractValidator<MarkAsStudentCommand>
{
    public MarkAsStudentCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.FamilyMemberId)
            .NotNull()
            .WithMessage(_ => localizer["FamilyMemberIdRequired"]);

        RuleFor(x => x.MarkedByUserId)
            .NotNull()
            .WithMessage(_ => localizer["MarkedByUserIdRequired"]);
    }
}
