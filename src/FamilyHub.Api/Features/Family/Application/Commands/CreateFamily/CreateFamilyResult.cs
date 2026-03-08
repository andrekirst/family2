using FamilyHub.Common.Domain.ValueObjects;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.Api.Features.Family.Application.Commands.CreateFamily;

/// <summary>
/// Result of family creation command.
/// </summary>
public sealed record CreateFamilyResult(
    FamilyId FamilyId,
    FamilyEntity CreatedFamily
);
