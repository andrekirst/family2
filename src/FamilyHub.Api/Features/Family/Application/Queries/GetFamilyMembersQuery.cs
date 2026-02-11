using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries;

/// <summary>
/// Query to get all members of the current user's family.
/// </summary>
public sealed record GetFamilyMembersQuery(
    ExternalUserId ExternalUserId
) : IQuery<List<UserDto>>;
