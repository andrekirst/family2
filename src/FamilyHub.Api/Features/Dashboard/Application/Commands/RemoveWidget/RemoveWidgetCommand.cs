using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.RemoveWidget;

public sealed record RemoveWidgetCommand(
    DashboardWidgetId WidgetId
) : ICommand<bool>;
