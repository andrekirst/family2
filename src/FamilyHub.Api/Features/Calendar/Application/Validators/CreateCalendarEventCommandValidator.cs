using FamilyHub.Api.Features.Calendar.Application.Commands;
using FluentValidation;

namespace FamilyHub.Api.Features.Calendar.Application.Validators;

public sealed class CreateCalendarEventCommandValidator : AbstractValidator<CreateCalendarEventCommand>
{
    public CreateCalendarEventCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotNull()
            .WithMessage("Event title is required");

        RuleFor(x => x.FamilyId)
            .NotNull()
            .WithMessage("Family ID is required");

        RuleFor(x => x.CreatedBy)
            .NotNull()
            .WithMessage("Creator ID is required");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time");
    }
}
