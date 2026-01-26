using FamilyHub.Modules.UserProfile.Application.Queries.GetMyProfile;
using FamilyHub.Modules.UserProfile.Application.Queries.GetUserProfile;
using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using HotChocolate.Authorization;
using MediatR;

namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Queries;

/// <summary>
/// GraphQL queries for user profile operations.
/// </summary>
[ExtendObjectType("Query")]
public sealed class UserProfileQueries
{
    /// <summary>
    /// Gets the current user's profile.
    /// Returns null if no profile exists.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Get the current user's profile")]
    public async Task<UserProfileDto?> MyProfile(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetMyProfileQuery();
        var result = await mediator.Send<GetMyProfileResult?>(query, cancellationToken);

        if (result == null)
        {
            return null;
        }

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
                BirthdayVisibility = result.FieldVisibility.BirthdayVisibility.Value,
                PronounsVisibility = result.FieldVisibility.PronounsVisibility.Value,
                PreferencesVisibility = result.FieldVisibility.PreferencesVisibility.Value
            },
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };
    }

    /// <summary>
    /// Gets another user's profile with visibility filtering.
    /// Returns null if no profile exists or user not found.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Get another user's profile (respects visibility settings)")]
    public async Task<PublicUserProfileDto?> UserProfile(
        Guid userId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetUserProfileQuery(UserId.From(userId));
        var result = await mediator.Send<GetUserProfileResult?>(query, cancellationToken);

        if (result == null)
        {
            return null;
        }

        return new PublicUserProfileDto
        {
            Id = result.ProfileId.Value,
            UserId = result.UserId.Value,
            DisplayName = result.DisplayName.Value,
            Birthday = result.Birthday?.Value,
            Age = result.Age,
            Pronouns = result.Pronouns?.Value,
            Preferences = result.Preferences != null
                ? new ProfilePreferencesDto
                {
                    Language = result.Preferences.Language,
                    Timezone = result.Preferences.Timezone,
                    DateFormat = result.Preferences.DateFormat
                }
                : null,
            CreatedAt = result.CreatedAt
        };
    }
}
