using FamilyHub.Api.Common.Search;

namespace FamilyHub.Api.Features.Messaging.Application.Search;

public sealed class MessagingCommandPaletteProvider : ICommandPaletteProvider
{
    public string ModuleName => "messaging";

    public IReadOnlyList<CommandDescriptor> GetCommands() =>
    [
        new CommandDescriptor(
            Label: "Open Messages",
            Description: "Open family messages",
            Keywords: ["messages", "chat", "nachrichten", "chatten"],
            Route: "/messages",
            RequiredPermissions: [],
            Icon: "message-circle",
            Group: "messaging",
            LabelDe: "Nachrichten öffnen",
            DescriptionDe: "Familiennachrichten öffnen"),

        new CommandDescriptor(
            Label: "New Message",
            Description: "Send a new message",
            Keywords: ["new message", "send", "neue nachricht", "schreiben"],
            Route: "/messages?action=create",
            RequiredPermissions: [],
            Icon: "message-square-plus",
            Group: "messaging",
            LabelDe: "Neue Nachricht",
            DescriptionDe: "Neue Nachricht schreiben")
    ];
}
