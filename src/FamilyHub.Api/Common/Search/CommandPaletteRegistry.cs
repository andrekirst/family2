namespace FamilyHub.Api.Common.Search;

public sealed class CommandPaletteRegistry : ICommandPaletteRegistry
{
    private readonly List<CommandDescriptor> _commands = [];

    public void RegisterProvider(ICommandPaletteProvider provider)
    {
        _commands.AddRange(provider.GetCommands());
    }

    public IReadOnlyList<CommandDescriptor> GetAllCommands() =>
        _commands.AsReadOnly();

    public IReadOnlyList<CommandDescriptor> GetCommandsByModule(string moduleName) =>
        _commands.Where(c => c.Group == moduleName).ToList().AsReadOnly();
}
