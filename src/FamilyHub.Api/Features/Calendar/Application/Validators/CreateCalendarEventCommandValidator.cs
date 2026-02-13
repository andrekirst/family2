using FamilyHub.Api.Features.Calendar.Application.Commands;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Calendar.Application.Validators;

public sealed class CreateCalendarEventCommandValidator : AbstractValidator<CreateCalendarEventCommand>
{
    public CreateCalendarEventCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Title)
            .NotNull()
            .WithMessage(_ => localizer["EventTitleRequired"]);

        RuleFor(x => x.FamilyId)
            .NotNull()
            .WithMessage(_ => localizer["FamilyIdRequired"]);

        RuleFor(x => x.CreatedBy)
            .NotNull()
            .WithMessage(_ => localizer["CreatorIdRequired"]);

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage(_ => localizer["EndTimeMustBeAfterStartTime"]);
    }
}
