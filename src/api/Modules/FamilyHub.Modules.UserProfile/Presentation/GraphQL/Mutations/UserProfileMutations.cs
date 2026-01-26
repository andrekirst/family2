using FamilyHub.Infrastructure.GraphQL.Interceptors;
using FamilyHub.Modules.UserProfile.Application.Commands.UpdateUserProfile;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Domain;
using HotChocolate.Authorization;
using MediatR;

namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for user profile operations.
/// Authorization is applied via [Authorize] attribute.
/// </summary>
[ExtendObjectType("Mutation")]
public sealed class UserProfileMutations : IRequireAuthentication
{
    /// <summary>
    /// Updates the current user's profile.
    /// Creates a new profile if one doesn't exist.
    /// </summary>
    [Authorize]
    [DefaultMutationErrors]
    [UseMutationConvention]
    [GraphQLDescription("Update the current user's profile (creates if not exists)")]
    public async Task<UpdateUserProfileDto> UpdateUserProfile(
        UpdateUserProfileInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        // Map input (primitives) → command (value objects)
        var preferences = input.Preferences != null
            ? ProfilePreferences.Create(
                input.Preferences.Language,
                input.Preferences.Timezone,
                input.Preferences.DateFormat)
            : null;

        var fieldVisibility = input.FieldVisibility != null
            ? ProfileFieldVisibility.Create(
                VisibilityLevel.From(input.FieldVisibility.BirthdayVisibility ?? "family"),
                VisibilityLevel.From(input.FieldVisibility.PronounsVisibility ?? "family"),
                VisibilityLevel.From(input.FieldVisibility.PreferencesVisibility ?? "hidden"))
            : null;

        var command = new UpdateUserProfileCommand(
            DisplayName: DisplayName.From(input.DisplayName),
            Birthday: input.Birthday.HasValue ? Birthday.From(input.Birthday.Value) : null,
            Pronouns: !string.IsNullOrWhiteSpace(input.Pronouns) ? Pronouns.From(input.Pronouns) : null,
            Preferences: preferences,
            FieldVisibility: fieldVisibility
        );

        // Execute command
        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<UpdateUserProfileResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(result.Error)
                    .SetCode("PROFILE_UPDATE_FAILED")
                    .Build());
        }

        // Map result → DTO
        return new UpdateUserProfileDto
        {
            ProfileId = result.Value.ProfileId.Value,
            DisplayName = result.Value.DisplayName.Value,
            UpdatedAt = result.Value.UpdatedAt,
            IsNewProfile = result.Value.IsNewProfile
        };
    }
}
