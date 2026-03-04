using FamilyHub.Api.Common.Search;

namespace FamilyHub.Api.Features.Family.Application.Search;

public sealed class FamilyCommandPaletteProvider : ICommandPaletteProvider
{
    public string ModuleName => "family";

    public IReadOnlyList<CommandDescriptor> GetCommands() =>
    [
        new CommandDescriptor(
            Label: "Invite Member",
            Description: "Send an invitation to join your family",
            Keywords: ["invite", "add", "member", "einladen", "mitglied"],
            Route: "/family?action=invite",
            RequiredPermissions: ["family:invite"],
            Icon: "user-plus",
            Group: "family",
            LabelDe: "Mitglied einladen",
            DescriptionDe: "Einladung zur Familie senden"),

        new CommandDescriptor(
            Label: "Family Settings",
            Description: "Manage your family settings",
            Keywords: ["settings", "edit", "family", "einstellungen", "familie"],
            Route: "/family/settings",
            RequiredPermissions: ["family:edit"],
            Icon: "settings",
            Group: "family",
            LabelDe: "Familieneinstellungen",
            DescriptionDe: "Familieneinstellungen verwalten"),

        new CommandDescriptor(
            Label: "View Members",
            Description: "See all family members",
            Keywords: ["members", "list", "view", "mitglieder", "anzeigen"],
            Route: "/family",
            RequiredPermissions: [],
            Icon: "users",
            Group: "family",
            LabelDe: "Mitglieder anzeigen",
            DescriptionDe: "Alle Familienmitglieder anzeigen")
    ];
}
