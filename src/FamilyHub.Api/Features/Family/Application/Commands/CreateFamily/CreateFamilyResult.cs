using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.CreateFamily;

/// <summary>
/// Result of family creation command.
/// </summary>
public sealed record CreateFamilyResult(
    FamilyId FamilyId
);
