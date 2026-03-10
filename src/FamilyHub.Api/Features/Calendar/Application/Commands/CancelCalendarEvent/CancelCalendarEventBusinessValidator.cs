using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Calendar.Application.Commands.CancelCalendarEvent;

public sealed class CancelCalendarEventBusinessValidator : AbstractValidator<CancelCalendarEventCommand>, IBusinessValidator<CancelCalendarEventCommand>
{
    public CancelCalendarEventBusinessValidator(
        ICalendarEventRepository repository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
                await repository.ExistsByIdAsync(command.CalendarEventId, cancellationToken))
            .WithErrorCode(DomainErrorCodes.CalendarEventNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.CalendarEventNotFound].Value);

        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
                !await repository.IsCancelledAsync(command.CalendarEventId, cancellationToken))
            .WithErrorCode(DomainErrorCodes.EventAlreadyCancelled)
            .WithMessage(_ => localizer[DomainErrorCodes.EventAlreadyCancelled].Value);
    }
}
