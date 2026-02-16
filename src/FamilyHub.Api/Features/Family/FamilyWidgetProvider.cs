using FamilyHub.Api.Common.Widgets;

namespace FamilyHub.Api.Features.Family;

public sealed class FamilyWidgetProvider : IWidgetProvider
{
    public string ModuleName => "family";

    public IReadOnlyList<WidgetDescriptor> GetWidgets() =>
    [
        new WidgetDescriptor(
            WidgetTypeId: "family:overview",
            Module: "family",
            Name: "Family Overview",
            Description: "Shows family members and their roles",
            DefaultWidth: 6, DefaultHeight: 4,
            MinWidth: 4, MinHeight: 3,
            MaxWidth: 12, MaxHeight: 8,
            ConfigSchema: null,
            RequiredPermissions: ["family:view"]),

        new WidgetDescriptor(
            WidgetTypeId: "family:pending-invitations",
            Module: "family",
            Name: "Pending Invitations",
            Description: "Accept or decline family invitations",
            DefaultWidth: 6, DefaultHeight: 4,
            MinWidth: 4, MinHeight: 3,
            MaxWidth: 12, MaxHeight: 8,
            ConfigSchema: null,
            RequiredPermissions: []),

        new WidgetDescriptor(
            WidgetTypeId: "family:upcoming-events",
            Module: "family",
            Name: "Upcoming Events",
            Description: "Calendar events coming up soon",
            DefaultWidth: 6, DefaultHeight: 4,
            MinWidth: 4, MinHeight: 3,
            MaxWidth: 12, MaxHeight: 8,
            ConfigSchema: null,
            RequiredPermissions: ["family:view"])
    ];
}
