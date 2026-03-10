using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Calendar.Application.Commands.UpdateCalendarEvent;

public sealed class UpdateCalendarEventCommandValidator : AbstractValidator<UpdateCalendarEventCommand>, IInputValidator<UpdateCalendarEventCommand>
{
    public UpdateCalendarEventCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.CalendarEventId)
            .NotNull()
            .WithMessage(_ => localizer["CalendarEventIdRequired"]);

        RuleFor(x => x.Title)
            .NotNull()
            .WithMessage(_ => localizer["EventTitleRequired"]);

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage(_ => localizer["EndTimeMustBeAfterStartTime"]);
    }
}
