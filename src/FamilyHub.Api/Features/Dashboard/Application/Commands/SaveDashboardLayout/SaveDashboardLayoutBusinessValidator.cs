using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Common.Widgets;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.SaveDashboardLayout;

public sealed class SaveDashboardLayoutBusinessValidator : AbstractValidator<SaveDashboardLayoutCommand>, IBusinessValidator<SaveDashboardLayoutCommand>
{
    public SaveDashboardLayoutBusinessValidator(
        IWidgetRegistry widgetRegistry,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleForEach(x => x.Widgets)
            .Must(widget => widgetRegistry.IsValidWidget(widget.WidgetType.Value))
            .WithErrorCode(DomainErrorCodes.InvalidWidgetType)
            .WithMessage(_ => localizer[DomainErrorCodes.InvalidWidgetType].Value);
    }
}
