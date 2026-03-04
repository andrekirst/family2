namespace FamilyHub.Api.Common.Search;

public interface ICommandPaletteProvider
{
    string ModuleName { get; }
    IReadOnlyList<CommandDescriptor> GetCommands();
}
