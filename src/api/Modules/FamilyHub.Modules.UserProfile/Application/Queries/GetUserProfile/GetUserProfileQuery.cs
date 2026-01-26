using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Application.Queries.GetUserProfile;

/// <summary>
/// Query to get another user's profile.
/// Respects visibility settings based on the relationship between users.
/// </summary>
public sealed record GetUserProfileQuery(
    UserId UserId
) : IQuery<GetUserProfileResult?>, IRequireAuthentication;
