using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.EventChain.Domain.Entities;

public sealed class ChainDefinition : AggregateRoot<ChainDefinitionId>
{
    private readonly List<ChainDefinitionStep> _steps = [];

#pragma warning disable CS8618
    private ChainDefinition() { }
#pragma warning restore CS8618

    public static ChainDefinition Create(
        ChainName name,
        string? description,
        FamilyId familyId,
        UserId createdByUserId,
        string triggerEventType,
        string triggerModule,
        string? triggerDescription,
        string? triggerOutputSchema,
        DateTimeOffset utcNow)
    {
        return new ChainDefinition
        {
            Id = ChainDefinitionId.New(),
            Name = name,
            Description = description,
            FamilyId = familyId,
            CreatedByUserId = createdByUserId,
            IsEnabled = true,
            IsTemplate = false,
            TriggerEventType = triggerEventType,
            TriggerModule = triggerModule,
            TriggerDescription = triggerDescription,
            TriggerOutputSchema = triggerOutputSchema,
            Version = 1,
            CreatedAt = utcNow.UtcDateTime,
            UpdatedAt = utcNow.UtcDateTime
        };
    }

    public static ChainDefinition CreateTemplate(
        ChainName name,
        string? description,
        FamilyId familyId,
        UserId createdByUserId,
        string templateName,
        string triggerEventType,
        string triggerModule,
        string? triggerDescription,
        string? triggerOutputSchema,
        DateTimeOffset utcNow)
    {
        var definition = Create(
            name, description, familyId, createdByUserId,
            triggerEventType, triggerModule, triggerDescription, triggerOutputSchema, utcNow);

        definition.IsTemplate = true;
        definition.TemplateName = templateName;

        return definition;
    }

    public ChainName Name { get; private set; }
    public string? Description { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public UserId CreatedByUserId { get; private set; }
    public bool IsEnabled { get; private set; }
    public bool IsTemplate { get; private set; }
    public string? TemplateName { get; private set; }
    public string TriggerEventType { get; private set; }
    public string TriggerModule { get; private set; }
    public string? TriggerDescription { get; private set; }
    public string? TriggerOutputSchema { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<ChainDefinitionStep> Steps => _steps.AsReadOnly();

    public void AddStep(ChainDefinitionStep step)
    {
        if (_steps.Any(s => s.Alias == step.Alias))
            throw new DomainException($"Step alias '{step.Alias.Value}' already exists in this chain", DomainErrorCodes.DuplicateStepAlias);

        _steps.Add(step);
    }

    public void ClearSteps()
    {
        _steps.Clear();
    }

    public void Update(ChainName? name, string? description, bool? isEnabled, DateTimeOffset utcNow)
    {
        if (name.HasValue) Name = name.Value;
        if (description is not null) Description = description;
        if (isEnabled.HasValue) IsEnabled = isEnabled.Value;

        Version++;
        UpdatedAt = utcNow.UtcDateTime;
    }

    public void Enable(DateTimeOffset utcNow)
    {
        IsEnabled = true;
        UpdatedAt = utcNow.UtcDateTime;
    }

    public void Disable(DateTimeOffset utcNow)
    {
        IsEnabled = false;
        UpdatedAt = utcNow.UtcDateTime;
    }
}
