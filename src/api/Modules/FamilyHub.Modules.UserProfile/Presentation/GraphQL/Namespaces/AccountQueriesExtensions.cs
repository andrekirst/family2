using FamilyHub.Modules.Auth.Presentation.GraphQL.Namespaces;
using FamilyHub.Modules.UserProfile.Application.Queries.GetMyProfile;
using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using MediatR;

namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Namespaces;

/// <summary>
/// Extends AccountQueries with profile-related queries.
/// Access pattern: query { account { myProfile { ... } } }
/// </summary>
/// <remarks>
/// <para>
/// This extension adds profile queries to the account namespace, providing
/// a user-centric query structure for account-related data.
/// </para>
/// <para>
/// Profile field visibility is controlled by the @visible directive based on
/// the viewer's relationship to the profile owner (owner, family, public).
/// </para>
/// </remarks>
[ExtendObjectType(typeof(AccountQueries))]
public sealed class AccountQueriesExtensions
{
    /// <summary>
    /// Gets the current user's profile.
    /// Returns null if no profile exists.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's profile or null.</returns>
    [Authorize]
    [GraphQLDescription("Get the current user's profile.")]
    public async Task<UserProfileDto?> MyProfile(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send<GetMyProfileResult?>(new GetMyProfileQuery(), cancellationToken);

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
}
