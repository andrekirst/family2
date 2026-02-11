namespace FamilyHub.EventChain.Infrastructure.Registry;

public sealed record ActionDescriptor(
    string ActionType,
    string Module,
    string Name,
    string Description,
    string Version,
    string InputSchema,
    string OutputSchema,
    bool IsCompensatable,
    bool IsDeprecated = false);
