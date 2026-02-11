namespace FamilyHub.EventChain.Infrastructure.Registry;

public interface IChainRegistry
{
    void RegisterPlugin(IChainPlugin plugin);
    IReadOnlyList<TriggerDescriptor> GetTriggers();
    IReadOnlyList<ActionDescriptor> GetActions();
    IReadOnlyList<ActionDescriptor> GetActionsCompatibleWith(string triggerEventType);
    TriggerDescriptor? GetTrigger(string eventType);
    ActionDescriptor? GetAction(string actionType, string version);
    IChainActionHandler? GetActionHandler(string actionType, string version);
    bool IsValidTrigger(string eventType);
    bool IsValidAction(string actionType, string version);
}
