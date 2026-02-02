using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands;

/// <summary>
/// Result of family creation command.
/// </summary>
public sealed record CreateFamilyResult(
    FamilyId FamilyId
);