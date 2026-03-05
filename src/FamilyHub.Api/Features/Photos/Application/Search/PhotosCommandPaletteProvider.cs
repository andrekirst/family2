using FamilyHub.Api.Common.Search;

namespace FamilyHub.Api.Features.Photos.Application.Search;

public sealed class PhotosCommandPaletteProvider : ICommandPaletteProvider
{
    public string ModuleName => "photos";

    public IReadOnlyList<CommandDescriptor> GetCommands() =>
    [
        new CommandDescriptor(
            Label: "View Photos",
            Description: "Browse the photo gallery",
            Keywords: ["photos", "gallery", "fotos", "galerie"],
            Route: "/photos",
            RequiredPermissions: [],
            Icon: "camera",
            Group: "photos",
            LabelDe: "Fotos anzeigen",
            DescriptionDe: "Fotogalerie durchsuchen"),

        new CommandDescriptor(
            Label: "Upload Photo",
            Description: "Upload a new photo",
            Keywords: ["upload photo", "foto hochladen"],
            Route: "/photos?action=upload",
            RequiredPermissions: [],
            Icon: "upload",
            Group: "photos",
            LabelDe: "Foto hochladen",
            DescriptionDe: "Neues Foto hochladen")
    ];
}
