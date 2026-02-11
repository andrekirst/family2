using System.ComponentModel.DataAnnotations;

namespace FamilyHub.Api.Features.EventChain.Models;

public sealed record CreateChainDefinitionInput
{
    [Required]
    public required string Name { get; init; }
    public string? Description { get; init; }
    [Required]
    public required string TriggerEventType { get; init; }
    [Required]
    public required List<CreateStepInput> Steps { get; init; }
    public bool IsEnabled { get; init; } = true;
}

public sealed record CreateStepInput
{
    [Required]
    public required string Alias { get; init; }
    [Required]
    public required string Name { get; init; }
    [Required]
    public required string ActionType { get; init; }
    [Required]
    public required string ActionVersion { get; init; }
    [Required]
    public required string InputMappings { get; init; }
    public string? Condition { get; init; }
    public required int Order { get; init; }
}

public sealed record UpdateChainDefinitionInput
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsEnabled { get; init; }
    public List<CreateStepInput>? Steps { get; init; }
}

public sealed record InstallTemplateInput
{
    [Required]
    public required string TemplateName { get; init; }
    public string? CustomName { get; init; }
    public bool IsEnabled { get; init; } = true;
}
