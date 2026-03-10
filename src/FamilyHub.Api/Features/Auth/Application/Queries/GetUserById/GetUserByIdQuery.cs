using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Models;

namespace FamilyHub.Api.Features.Auth.Application.Queries.GetUserById;

/// <summary>
/// Query to get a user by their unique identifier.
/// Requires family membership — callers can only look up users within their own family.
/// </summary>
public sealed record GetUserByIdQuery(
    UserId TargetUserId
) : IReadOnlyQuery<UserDto?>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
