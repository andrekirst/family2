using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.SaveDashboardLayout;

public sealed record SaveDashboardLayoutCommand(
    DashboardLayoutName Name,
    bool IsShared,
    IReadOnlyList<WidgetPositionData> Widgets
) : ICommand<SaveDashboardLayoutResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}

public sealed record WidgetPositionData(
    WidgetTypeId WidgetType,
    int X, int Y,
    int Width, int Height,
    int SortOrder,
    string? ConfigJson);
