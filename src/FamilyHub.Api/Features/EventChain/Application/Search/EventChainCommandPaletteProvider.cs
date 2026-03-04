using FamilyHub.Api.Common.Search;

namespace FamilyHub.Api.Features.EventChain.Application.Search;

public sealed class EventChainCommandPaletteProvider : ICommandPaletteProvider
{
    public string ModuleName => "event-chains";

    public IReadOnlyList<CommandDescriptor> GetCommands() =>
    [
        new CommandDescriptor(
            Label: "View Automations",
            Description: "View automation workflows",
            Keywords: ["automations", "workflows", "chains", "automatisierung"],
            Route: "/event-chains",
            RequiredPermissions: [],
            Icon: "zap",
            Group: "event-chains",
            LabelDe: "Automatisierungen anzeigen",
            DescriptionDe: "Automatisierungs-Workflows anzeigen"),

        new CommandDescriptor(
            Label: "Create Automation",
            Description: "Create a new automation",
            Keywords: ["create automation", "neue automatisierung", "erstellen"],
            Route: "/event-chains?action=create",
            RequiredPermissions: [],
            Icon: "plus",
            Group: "event-chains",
            LabelDe: "Automatisierung erstellen",
            DescriptionDe: "Neue Automatisierung erstellen")
    ];
}
