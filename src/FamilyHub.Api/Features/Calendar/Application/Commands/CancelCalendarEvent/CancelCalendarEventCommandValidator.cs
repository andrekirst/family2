using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Calendar.Application.Commands.CancelCalendarEvent;

public sealed class CancelCalendarEventCommandValidator : AbstractValidator<CancelCalendarEventCommand>, IInputValidator<CancelCalendarEventCommand>
{
    public CancelCalendarEventCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.CalendarEventId)
            .NotNull()
            .WithMessage(_ => localizer["CalendarEventIdRequired"]);
    }
}
