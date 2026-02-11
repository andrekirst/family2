namespace FamilyHub.EventChain.Infrastructure.Registry;

public sealed class ChainRegistry : IChainRegistry
{
    private readonly List<TriggerDescriptor> _triggers = [];
    private readonly List<ActionDescriptor> _actions = [];
    private readonly Dictionary<string, IChainActionHandler> _actionHandlers = new();

    public void RegisterPlugin(IChainPlugin plugin)
    {
        _triggers.AddRange(plugin.GetTriggers());
        _actions.AddRange(plugin.GetActions());

        foreach (var handler in plugin.GetActionHandlers())
        {
            var key = BuildActionKey(handler.ActionType, handler.Version);
            _actionHandlers[key] = handler;
        }
    }

    public IReadOnlyList<TriggerDescriptor> GetTriggers() => _triggers.AsReadOnly();

    public IReadOnlyList<ActionDescriptor> GetActions() =>
        _actions.Where(a => !a.IsDeprecated).ToList().AsReadOnly();

    public IReadOnlyList<ActionDescriptor> GetActionsCompatibleWith(string triggerEventType)
    {
        // In V1, all non-deprecated actions are compatible with any trigger.
        // Type-compatible validation happens at chain definition save time via input/output schema matching.
        return GetActions();
    }

    public TriggerDescriptor? GetTrigger(string eventType) =>
        _triggers.FirstOrDefault(t => t.EventType == eventType);

    public ActionDescriptor? GetAction(string actionType, string version) =>
        _actions.FirstOrDefault(a => a.ActionType == actionType && a.Version == version);

    public IChainActionHandler? GetActionHandler(string actionType, string version)
    {
        var key = BuildActionKey(actionType, version);
        return _actionHandlers.GetValueOrDefault(key);
    }

    public bool IsValidTrigger(string eventType) =>
        _triggers.Any(t => t.EventType == eventType);

    public bool IsValidAction(string actionType, string version) =>
        _actions.Any(a => a.ActionType == actionType && a.Version == version && !a.IsDeprecated);

    private static string BuildActionKey(string actionType, string version) =>
        $"{actionType}@{version}";
}
