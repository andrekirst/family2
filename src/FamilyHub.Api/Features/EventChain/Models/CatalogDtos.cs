namespace FamilyHub.Api.Features.EventChain.Models;

public sealed record ActionCatalogEntryDto(
    string ActionType,
    string Module,
    string Name,
    string Description,
    string Version,
    string InputSchema,
    string OutputSchema,
    bool IsCompensatable,
    bool IsDeprecated);

public sealed record TriggerCatalogEntryDto(
    string EventType,
    string Module,
    string Name,
    string Description,
    string OutputSchema);

public sealed record ChainTemplateDto(
    string Name,
    string Description,
    string Category,
    TriggerCatalogEntryDto Trigger,
    IReadOnlyList<StepDefinitionDto> Steps,
    string EstimatedTimeSaved,
    string MentalLoadReduction);

public sealed record ChainEntityMappingDto(
    Guid Id,
    Guid ChainExecutionId,
    string StepAlias,
    string EntityType,
    Guid EntityId,
    string Module,
    DateTime CreatedAt);
