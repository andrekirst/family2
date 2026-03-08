using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Models;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.AddWidget;

public sealed record AddWidgetCommand(
    DashboardId DashboardId,
    WidgetTypeId WidgetType,
    int X, int Y,
    int Width, int Height,
    string? ConfigJson,
    FamilyId FamilyId
) : ICommand<DashboardWidgetDto>, IFamilyScoped;
