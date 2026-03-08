using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.RemoveWidget;

public sealed class RemoveWidgetBusinessValidator : AbstractValidator<RemoveWidgetCommand>, IBusinessValidator<RemoveWidgetCommand>
{
    public RemoveWidgetBusinessValidator(
        IDashboardLayoutRepository dashboardRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x.WidgetId)
            .MustAsync(async (widgetId, ct) =>
                await dashboardRepository.ExistsByWidgetIdAsync(widgetId, ct))
            .WithErrorCode(DomainErrorCodes.WidgetNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.WidgetNotFound].Value);
    }
}
