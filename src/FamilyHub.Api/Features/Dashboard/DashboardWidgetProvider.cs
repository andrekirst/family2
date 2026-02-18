using FamilyHub.Api.Common.Widgets;

namespace FamilyHub.Api.Features.Dashboard;

public sealed class DashboardWidgetProvider : IWidgetProvider
{
    public string ModuleName => "dashboard";

    public IReadOnlyList<WidgetDescriptor> GetWidgets() =>
    [
        new WidgetDescriptor(
            WidgetTypeId: "dashboard:welcome",
            Module: "dashboard",
            Name: "Welcome",
            Description: "Greeting with quick actions",
            DefaultWidth: 12, DefaultHeight: 2,
            MinWidth: 6, MinHeight: 2,
            MaxWidth: 12, MaxHeight: 4,
            ConfigSchema: null,
            RequiredPermissions: [])
    ];
}
