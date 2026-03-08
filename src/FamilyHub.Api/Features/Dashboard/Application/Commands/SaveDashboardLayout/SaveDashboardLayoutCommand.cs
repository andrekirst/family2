using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.SaveDashboardLayout;

public sealed record SaveDashboardLayoutCommand(
    DashboardLayoutName Name,
    UserId? UserId,
    FamilyId FamilyId,
    bool IsShared,
    IReadOnlyList<WidgetPositionData> Widgets
) : ICommand<SaveDashboardLayoutResult>, IFamilyScoped;

public sealed record WidgetPositionData(
    WidgetTypeId WidgetType,
    int X, int Y,
    int Width, int Height,
    int SortOrder,
    string? ConfigJson);
