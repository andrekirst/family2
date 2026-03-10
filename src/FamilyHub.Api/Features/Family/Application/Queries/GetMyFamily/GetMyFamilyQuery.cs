using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetMyFamily;

/// <summary>
/// Query to get the current user's family.
/// </summary>
public sealed record GetMyFamilyQuery : IReadOnlyQuery<FamilyDto?>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
