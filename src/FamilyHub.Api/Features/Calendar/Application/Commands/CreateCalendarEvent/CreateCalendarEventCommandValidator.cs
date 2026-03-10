using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Calendar.Application.Commands.CreateCalendarEvent;

public sealed class CreateCalendarEventCommandValidator : AbstractValidator<CreateCalendarEventCommand>, IInputValidator<CreateCalendarEventCommand>
{
    public CreateCalendarEventCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Title)
            .NotNull()
            .WithMessage(_ => localizer["EventTitleRequired"]);

        RuleFor(x => x.FamilyId)
            .NotNull()
            .WithMessage(_ => localizer["FamilyIdRequired"]);

        RuleFor(x => x.UserId)
            .NotNull()
            .WithMessage(_ => localizer["CreatorIdRequired"]);

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage(_ => localizer["EndTimeMustBeAfterStartTime"]);
    }
}
