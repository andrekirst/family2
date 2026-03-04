using FamilyHub.Api.Common.Search;

namespace FamilyHub.Api.Features.Auth.Application.Search;

public sealed class ProfileCommandPaletteProvider : ICommandPaletteProvider
{
    public string ModuleName => "auth";

    public IReadOnlyList<CommandDescriptor> GetCommands() =>
    [
        new CommandDescriptor(
            Label: "My Profile",
            Description: "View and edit your profile",
            Keywords: ["profile", "account", "profil", "konto"],
            Route: "/profile",
            RequiredPermissions: [],
            Icon: "user",
            Group: "auth",
            LabelDe: "Mein Profil",
            DescriptionDe: "Profil anzeigen und bearbeiten"),

        new CommandDescriptor(
            Label: "Settings",
            Description: "App settings and preferences",
            Keywords: ["settings", "preferences", "einstellungen", "voreinstellungen"],
            Route: "/settings",
            RequiredPermissions: [],
            Icon: "settings",
            Group: "auth",
            LabelDe: "Einstellungen",
            DescriptionDe: "App-Einstellungen und Voreinstellungen")
    ];
}
