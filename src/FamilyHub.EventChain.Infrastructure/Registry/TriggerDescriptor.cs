namespace FamilyHub.EventChain.Infrastructure.Registry;

public sealed record TriggerDescriptor(
    string EventType,
    string Module,
    string Name,
    string Description,
    string OutputSchema);
