using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Calendar.Application.Commands.UpdateCalendarEvent;

public sealed class UpdateCalendarEventBusinessValidator : AbstractValidator<UpdateCalendarEventCommand>, IBusinessValidator<UpdateCalendarEventCommand>
{
    public UpdateCalendarEventBusinessValidator(
        ICalendarEventRepository repository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
                await repository.ExistsByIdAsync(command.CalendarEventId, ct))
            .WithErrorCode(DomainErrorCodes.CalendarEventNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.CalendarEventNotFound].Value);

        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
                !await repository.IsCancelledAsync(command.CalendarEventId, ct))
            .WithErrorCode(DomainErrorCodes.CannotUpdateCancelledEvent)
            .WithMessage(_ => localizer[DomainErrorCodes.CannotUpdateCancelledEvent].Value);
    }
}
