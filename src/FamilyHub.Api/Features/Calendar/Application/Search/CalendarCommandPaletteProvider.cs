using FamilyHub.Api.Common.Search;

namespace FamilyHub.Api.Features.Calendar.Application.Search;

public sealed class CalendarCommandPaletteProvider : ICommandPaletteProvider
{
    public string ModuleName => "calendar";

    public IReadOnlyList<CommandDescriptor> GetCommands() =>
    [
        new CommandDescriptor(
            Label: "Create Event",
            Description: "Create a new calendar event",
            Keywords: ["create", "event", "new", "termin", "erstellen", "kalender"],
            Route: "/family/calendar?action=create",
            RequiredPermissions: [],
            Icon: "calendar-plus",
            Group: "calendar",
            LabelDe: "Termin erstellen",
            DescriptionDe: "Neuen Kalendertermin erstellen"),

        new CommandDescriptor(
            Label: "View Calendar",
            Description: "Open the family calendar",
            Keywords: ["calendar", "view", "events", "kalender", "termine", "anzeigen"],
            Route: "/family/calendar",
            RequiredPermissions: [],
            Icon: "calendar",
            Group: "calendar",
            LabelDe: "Kalender anzeigen",
            DescriptionDe: "Familienkalender öffnen")
    ];
}
