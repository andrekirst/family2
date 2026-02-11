namespace FamilyHub.Api.Features.EventChain.Models;

public sealed record CreateChainDefinitionPayload(
    ChainDefinitionDto? ChainDefinition,
    IReadOnlyList<UserError>? Errors = null);

public sealed record UpdateChainDefinitionPayload(
    ChainDefinitionDto? ChainDefinition,
    IReadOnlyList<UserError>? Errors = null);

public sealed record DeleteChainDefinitionPayload(
    bool Success,
    IReadOnlyList<UserError>? Errors = null);

public sealed record UserError(
    string Message,
    string Code,
    string? Field = null);
