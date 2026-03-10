using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.UpdateWidgetConfig;

public sealed class UpdateWidgetConfigBusinessValidator : AbstractValidator<UpdateWidgetConfigCommand>, IBusinessValidator<UpdateWidgetConfigCommand>
{
    public UpdateWidgetConfigBusinessValidator(
        IDashboardLayoutRepository dashboardRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x.WidgetId)
            .MustAsync(async (widgetId, cancellationToken) =>
                await dashboardRepository.ExistsByWidgetIdAsync(widgetId, cancellationToken))
            .WithErrorCode(DomainErrorCodes.WidgetNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.WidgetNotFound].Value);
    }
}
