namespace FamilyHub.EventChain.Infrastructure.Registry;

public interface IChainPlugin
{
    string ModuleName { get; }
    IReadOnlyList<TriggerDescriptor> GetTriggers();
    IReadOnlyList<ActionDescriptor> GetActions();
    IReadOnlyList<IChainActionHandler> GetActionHandlers();
}
