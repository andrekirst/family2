using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Models;

namespace FamilyHub.Api.Features.Auth.Application.Queries.GetCurrentUser;

/// <summary>
/// Query to get a user by their external OAuth ID.
/// Used to get the current authenticated user.
/// </summary>
public sealed record GetCurrentUserQuery(
    ExternalUserId ExternalUserId
) : IQuery<UserDto?>;
