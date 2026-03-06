using FamilyHub.Api.Common.Search;

namespace FamilyHub.Api.Features.School.Application.Search;

public sealed class SchoolCommandPaletteProvider : ICommandPaletteProvider
{
    public string ModuleName => "school";

    public IReadOnlyList<CommandDescriptor> GetCommands() =>
    [
        new CommandDescriptor(
            Label: "View Students",
            Description: "View all students in the family",
            Keywords: ["students", "school", "view", "list", "schüler", "anzeigen"],
            Route: "/school",
            RequiredPermissions: [],
            Icon: "graduation-cap",
            Group: "school",
            LabelDe: "Schüler anzeigen",
            DescriptionDe: "Alle Schüler der Familie anzeigen"),

        new CommandDescriptor(
            Label: "Go to School",
            Description: "Open the school section",
            Keywords: ["school", "go", "open", "navigate", "schule", "öffnen"],
            Route: "/school",
            RequiredPermissions: [],
            Icon: "graduation-cap",
            Group: "school",
            LabelDe: "Schule öffnen",
            DescriptionDe: "Den Schulbereich öffnen"),

        new CommandDescriptor(
            Label: "Mark as Student",
            Description: "Mark a family member as a student",
            Keywords: ["mark", "student", "add", "markieren", "schüler", "hinzufügen"],
            Route: "/school?action=mark-student",
            RequiredPermissions: ["school:manage-students"],
            Icon: "graduation-cap",
            Group: "school",
            LabelDe: "Als Schüler markieren",
            DescriptionDe: "Ein Familienmitglied als Schüler markieren")
    ];
}
