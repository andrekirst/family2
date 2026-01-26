using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;

namespace FamilyHub.Modules.UserProfile.Application.Queries.GetMyProfile;

/// <summary>
/// Query to get the current user's profile.
/// Returns null if no profile exists.
/// </summary>
public sealed record GetMyProfileQuery : IQuery<GetMyProfileResult?>, IRequireAuthentication;
