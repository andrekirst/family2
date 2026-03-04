using FamilyHub.Api.Common.Search;

namespace FamilyHub.Api.Features.Dashboard.Application.Search;

public sealed class DashboardCommandPaletteProvider : ICommandPaletteProvider
{
    public string ModuleName => "dashboard";

    public IReadOnlyList<CommandDescriptor> GetCommands() =>
    [
        new CommandDescriptor(
            Label: "Go to Dashboard",
            Description: "Open the home dashboard",
            Keywords: ["dashboard", "home", "startseite", "übersicht"],
            Route: "/dashboard",
            RequiredPermissions: [],
            Icon: "layout-dashboard",
            Group: "dashboard",
            LabelDe: "Zum Dashboard",
            DescriptionDe: "Startseite öffnen")
    ];
}
