using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Widgets;
using FamilyHub.Api.Features.Dashboard.Application.Mappers;
using FamilyHub.Api.Features.Dashboard.Models;

namespace FamilyHub.Api.Features.Dashboard.Application.Queries.GetAvailableWidgets;

public sealed class GetAvailableWidgetsQueryHandler(
    IWidgetRegistry widgetRegistry)
    : IQueryHandler<GetAvailableWidgetsQuery, IReadOnlyList<WidgetDescriptorDto>>
{
    public ValueTask<IReadOnlyList<WidgetDescriptorDto>> Handle(
        GetAvailableWidgetsQuery query,
        CancellationToken cancellationToken)
    {
        var widgets = widgetRegistry.GetAllWidgets()
            .Select(DashboardMapper.ToDto)
            .ToList();

        return ValueTask.FromResult<IReadOnlyList<WidgetDescriptorDto>>(widgets);
    }
}
