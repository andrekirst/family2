using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.ResetDashboard;

public sealed class ResetDashboardBusinessValidator : AbstractValidator<ResetDashboardCommand>, IBusinessValidator<ResetDashboardCommand>
{
    public ResetDashboardBusinessValidator(
        IDashboardLayoutRepository dashboardRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x.DashboardId)
            .MustAsync(async (dashboardId, ct) =>
            {
                var dashboard = await dashboardRepository.GetByIdAsync(dashboardId, ct);
                return dashboard is not null;
            })
            .WithErrorCode(DomainErrorCodes.DashboardNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.DashboardNotFound].Value);
    }
}
