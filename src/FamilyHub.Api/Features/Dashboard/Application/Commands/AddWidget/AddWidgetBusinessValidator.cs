using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Common.Widgets;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.AddWidget;

public sealed class AddWidgetBusinessValidator : AbstractValidator<AddWidgetCommand>, IBusinessValidator<AddWidgetCommand>
{
    public AddWidgetBusinessValidator(
        IDashboardLayoutRepository dashboardRepository,
        IWidgetRegistry widgetRegistry,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x.WidgetType)
            .Must(widgetType => widgetRegistry.IsValidWidget(widgetType.Value))
            .WithErrorCode(DomainErrorCodes.InvalidWidgetType)
            .WithMessage(_ => localizer[DomainErrorCodes.InvalidWidgetType].Value);

        RuleFor(x => x.DashboardId)
            .MustAsync(async (dashboardId, cancellationToken) =>
                await dashboardRepository.ExistsByIdAsync(dashboardId, cancellationToken))
            .WithErrorCode(DomainErrorCodes.DashboardNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.DashboardNotFound].Value);
    }
}
