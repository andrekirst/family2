using FamilyHub.Api.GraphQL.Types;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Family.Application.Queries.GetUserFamily;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.UserProfile.Application.Queries.GetMyProfile;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace FamilyHub.Api.Application.Services;

/// <summary>
/// Coordinates queries across Auth, Family, and UserProfile modules
/// for the consolidated "me" root query.
/// </summary>
/// <remarks>
/// This service uses:
/// <list type="bullet">
/// <item><description>MediatR for cross-module query dispatch</description></item>
/// <item><description>ICurrentUserService for user context</description></item>
/// <item><description>IUserLookupService for family membership checks</description></item>
/// <item><description>IRoleBasedVisibilityService for profile field filtering</description></item>
/// </list>
/// </remarks>
public sealed class MeQueryCoordinator(
    IMediator mediator,
    ICurrentUserService currentUserService,
    IUserLookupService userLookupService,
    IRoleBasedVisibilityService visibilityService) : IMeQueryCoordinator
{
    // Store visibility service for use in GetProfileWithVisibilityAsync once GetProfileByUserIdQuery is implemented
    private readonly IRoleBasedVisibilityService _visibilityService = visibilityService;

    /// <inheritdoc />
    public async Task<FamilyOrReasonResult> GetFamilyOrReasonAsync(CancellationToken cancellationToken)
    {
        // 1. Check if user has a family
        var familyResult = await mediator.Send<GetUserFamilyResult?>(
            new GetUserFamilyQuery(),
            cancellationToken);

        if (familyResult != null)
        {
            // User has a family - reconstitute and return
            var family = Family.Reconstitute(
                familyResult.FamilyId,
                FamilyName.From(familyResult.Name),
                familyResult.OwnerId,
                familyResult.CreatedAt,
                familyResult.UpdatedAt);

            return FamilyOrReasonResult.Success(family);
        }

        // 2. User has no family - determine the reason
        // Check for pending invitations
        var pendingCount = await GetPendingInvitationCountAsync(cancellationToken);
        if (pendingCount > 0)
        {
            return FamilyOrReasonResult.InvitePending(pendingCount);
        }

        // TODO: Check if user left a family (requires tracking left_at in user record)
        // For now, return NOT_CREATED if no pending invitations
        // In the future, we can check a "leftFamilyAt" field in the User aggregate
        // and return FamilyOrReasonResult.LeftFamily(leftAt) if set

        return FamilyOrReasonResult.NotCreated();
    }

    /// <inheritdoc />
    public async Task<bool> IsFamilyMemberAsync(Guid familyId, Guid userId, CancellationToken cancellationToken)
    {
        // Use IUserLookupService to check if user belongs to the family
        var userFamilyId = await userLookupService.GetUserFamilyIdAsync(
            UserId.From(userId),
            cancellationToken);

        return userFamilyId != null && userFamilyId.Value.Value == familyId;
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> GetProfileWithVisibilityAsync(
        Guid userId,
        FamilyRole viewerRole,
        CancellationToken cancellationToken)
    {
        // TODO: Create GetProfileByUserIdQuery for fetching other users' profiles
        // For now, this method is a placeholder that will be implemented
        // when the query is created. The coordinator infrastructure is ready.

        // Fetch the profile via MediatR (when query exists)
        // var profileResult = await mediator.Send<GetProfileByUserIdResult?>(
        //     new GetProfileByUserIdQuery(UserId.From(userId)),
        //     cancellationToken);

        // if (profileResult == null)
        // {
        //     return null;
        // }

        // Map to DTO and apply visibility filtering
        // var fullProfile = MapToDto(profileResult);
        // return visibilityService.FilterFieldsByRole(fullProfile, viewerRole);

        // Placeholder - will be implemented with GetProfileByUserIdQuery
        return null;
    }

    /// <inheritdoc />
    public async Task<int> GetPendingInvitationCountAsync(CancellationToken cancellationToken)
    {
        // Get the user's email to look up invitations
        var email = currentUserService.GetUserEmail();
        if (email == null)
        {
            return 0;
        }

        // TODO: Create a query to get invitations by email (received invitations)
        // For now, return 0 as the query doesn't exist yet
        // This will need a new query: GetReceivedInvitationsQuery
        // that looks up invitations by email where status = Pending

        // Placeholder - will be implemented when GetReceivedInvitationsQuery is created
        return 0;
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> GetMyProfileAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send<GetMyProfileResult?>(
            new GetMyProfileQuery(),
            cancellationToken);

        return result == null ? null : MapToDto(result);
    }

    private static UserProfileDto MapToDto(GetMyProfileResult result)
    {
        return new UserProfileDto
        {
            Id = result.ProfileId.Value,
            UserId = result.UserId.Value,
            DisplayName = result.DisplayName.Value,
            Birthday = result.Birthday?.Value,
            Age = result.Age,
            Pronouns = result.Pronouns?.Value,
            Preferences = new ProfilePreferencesDto
            {
                Language = result.Preferences.Language,
                Timezone = result.Preferences.Timezone,
                DateFormat = result.Preferences.DateFormat
            },
            FieldVisibility = new ProfileFieldVisibilityDto
            {
                BirthdayVisibility = MapVisibility(result.FieldVisibility.BirthdayVisibility),
                PronounsVisibility = MapVisibility(result.FieldVisibility.PronounsVisibility),
                PreferencesVisibility = MapVisibility(result.FieldVisibility.PreferencesVisibility)
            },
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };
    }

    private static string MapVisibility(VisibilityLevel visibility) =>
        visibility.Value.ToUpperInvariant() switch
        {
            "HIDDEN" => "HIDDEN",
            "FAMILY" => "FAMILY",
            "PUBLIC" => "PUBLIC",
            _ => "HIDDEN"
        };
}
