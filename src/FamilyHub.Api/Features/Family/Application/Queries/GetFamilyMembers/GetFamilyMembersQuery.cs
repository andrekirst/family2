using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetFamilyMembers;

/// <summary>
/// Query to get all members of the current user's family.
/// </summary>
public sealed record GetFamilyMembersQuery(
    ExternalUserId ExternalUserId
) : IQuery<List<UserDto>>;
