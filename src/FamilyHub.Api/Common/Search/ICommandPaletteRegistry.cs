namespace FamilyHub.Api.Common.Search;

public interface ICommandPaletteRegistry
{
    void RegisterProvider(ICommandPaletteProvider provider);
    IReadOnlyList<CommandDescriptor> GetAllCommands();
    IReadOnlyList<CommandDescriptor> GetCommandsByModule(string moduleName);
}
