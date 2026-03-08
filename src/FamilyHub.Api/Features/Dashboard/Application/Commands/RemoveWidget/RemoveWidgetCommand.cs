using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.RemoveWidget;

public sealed record RemoveWidgetCommand(
    DashboardWidgetId WidgetId,
    FamilyId FamilyId
) : ICommand<bool>, IFamilyScoped;
