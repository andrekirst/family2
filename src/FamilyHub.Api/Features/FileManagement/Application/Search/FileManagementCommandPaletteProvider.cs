using FamilyHub.Api.Common.Search;

namespace FamilyHub.Api.Features.FileManagement.Application.Search;

public sealed class FileManagementCommandPaletteProvider : ICommandPaletteProvider
{
    public string ModuleName => "files";

    public IReadOnlyList<CommandDescriptor> GetCommands() =>
    [
        new CommandDescriptor(
            Label: "Browse Files",
            Description: "Browse your family's files",
            Keywords: ["files", "browse", "dateien", "durchsuchen"],
            Route: "/files/browse",
            RequiredPermissions: [],
            Icon: "folder-open",
            Group: "files",
            LabelDe: "Dateien durchsuchen",
            DescriptionDe: "Dateien der Familie durchsuchen"),

        new CommandDescriptor(
            Label: "View Albums",
            Description: "View photo albums",
            Keywords: ["albums", "photos", "alben", "fotos"],
            Route: "/files/albums",
            RequiredPermissions: [],
            Icon: "image",
            Group: "files",
            LabelDe: "Alben anzeigen",
            DescriptionDe: "Fotoalben anzeigen"),

        new CommandDescriptor(
            Label: "Search Files",
            Description: "Search across all files",
            Keywords: ["search", "files", "suche", "dateien"],
            Route: "/files/search",
            RequiredPermissions: [],
            Icon: "search",
            Group: "files",
            LabelDe: "Dateien suchen",
            DescriptionDe: "Alle Dateien durchsuchen"),

        new CommandDescriptor(
            Label: "File Sharing",
            Description: "View shared files and links",
            Keywords: ["sharing", "shared", "teilen", "geteilt"],
            Route: "/files/sharing",
            RequiredPermissions: [],
            Icon: "share-2",
            Group: "files",
            LabelDe: "Dateifreigaben",
            DescriptionDe: "Geteilte Dateien und Links"),

        new CommandDescriptor(
            Label: "File Inbox",
            Description: "View received shared files",
            Keywords: ["inbox", "received", "posteingang"],
            Route: "/files/inbox",
            RequiredPermissions: [],
            Icon: "inbox",
            Group: "files",
            LabelDe: "Posteingang",
            DescriptionDe: "Empfangene geteilte Dateien"),

        new CommandDescriptor(
            Label: "Secure Notes",
            Description: "View encrypted secure notes",
            Keywords: ["notes", "secure", "notizen", "sicher"],
            Route: "/files/notes",
            RequiredPermissions: [],
            Icon: "lock",
            Group: "files",
            LabelDe: "Sichere Notizen",
            DescriptionDe: "Verschlüsselte Notizen anzeigen"),

        new CommandDescriptor(
            Label: "Upload File",
            Description: "Upload a new file",
            Keywords: ["upload", "hochladen"],
            Route: "/files/browse?action=upload",
            RequiredPermissions: [],
            Icon: "upload",
            Group: "files",
            LabelDe: "Datei hochladen",
            DescriptionDe: "Neue Datei hochladen"),

        new CommandDescriptor(
            Label: "Create Album",
            Description: "Create a new photo album",
            Keywords: ["create album", "album erstellen"],
            Route: "/files/albums?action=create",
            RequiredPermissions: [],
            Icon: "plus",
            Group: "files",
            LabelDe: "Album erstellen",
            DescriptionDe: "Neues Fotoalbum erstellen")
    ];
}
