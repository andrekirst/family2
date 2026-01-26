using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;

namespace FamilyHub.Modules.UserProfile.Application.Commands.UpdateUserProfile;

/// <summary>
/// Command to update or create a user profile.
/// If no profile exists for the user, one will be created.
/// </summary>
public sealed record UpdateUserProfileCommand(
    DisplayName DisplayName,
    Birthday? Birthday,
    Pronouns? Pronouns,
    ProfilePreferences? Preferences,
    ProfileFieldVisibility? FieldVisibility
) : ICommand<SharedKernel.Domain.Result<UpdateUserProfileResult>>,
    IRequireAuthentication;
