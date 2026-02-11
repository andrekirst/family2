using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Models;

namespace FamilyHub.Api.Features.Auth.Application.Queries;

/// <summary>
/// Query to get a user by their unique identifier.
/// </summary>
public sealed record GetUserByIdQuery(
    UserId UserId
) : IQuery<UserDto?>;
