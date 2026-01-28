using FamilyHub.Modules.UserProfile.Application.Commands.ApproveProfileChange;
using FamilyHub.Modules.UserProfile.Application.Commands.RejectProfileChange;
using FamilyHub.Modules.UserProfile.Application.Commands.UpdateUserProfile;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Inputs;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Domain.Exceptions;
using FamilyHub.SharedKernel.Presentation.GraphQL.Errors;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using MediatR;

namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Namespaces;

/// <summary>
/// GraphQL mutations for profile-related operations.
/// Extends the ProfileMutations namespace type.
/// </summary>
/// <remarks>
/// <para>
/// Uses HotChocolate mutation conventions for consistent error handling.
/// All mutations automatically include error union types via [Error] attributes.
/// </para>
/// <para>
/// Access pattern: mutation { account { profile { updateProfile(...) { data { ... } errors { ... } } } } }
/// </para>
/// <para>
/// Child profiles have restricted mutation capabilities with parental approval workflow.
/// </para>
/// </remarks>
[ExtendObjectType(typeof(ProfileMutations))]
public sealed class ProfileMutationsExtensions
{
    /// <summary>
    /// Updates the current user's profile.
    /// Creates a new profile if one doesn't exist.
    /// For child accounts, changes may require parental approval.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Update the current user's profile. For children, changes may require parental approval.")]
    [UseMutationConvention]
    [Error<ValidationError>]
    [Error<BusinessError>]
    public async Task<UpdateProfileResult> UpdateProfile(
        UpdateUserProfileInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
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
            FieldVisibility: fieldVisibility);

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<UpdateUserProfileResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("PROFILE_UPDATE_FAILED", result.Error);
        }

        return new UpdateProfileResult
        {
            ProfileId = result.Value.ProfileId.Value,
            DisplayName = result.Value.DisplayName.Value,
            UpdatedAt = result.Value.UpdatedAt,
            IsNewProfile = result.Value.IsNewProfile,
            RequiresApproval = result.Value.RequiresApproval,
            PendingChangesCount = result.Value.PendingChangesCount
        };
    }

    /// <summary>
    /// Approves a pending profile change request.
    /// Applies the change to the profile and marks the request as approved.
    /// Requires OWNER or ADMIN role (parent approving child's changes).
    /// </summary>
    [Authorize]
    [GraphQLDescription("Approve a pending profile change request. Requires Owner/Admin role.")]
    [UseMutationConvention]
    [Error<ValidationError>]
    [Error<BusinessError>]
    [Error<NotFoundError>]
    public async Task<ApproveChangeResult> ApproveChange(
        ApproveProfileChangeInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ApproveProfileChangeCommand(
            ChangeRequestId.From(input.RequestId));

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<ApproveProfileChangeResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("APPROVE_CHANGE_FAILED", result.Error);
        }

        return new ApproveChangeResult
        {
            RequestId = result.Value.RequestId.Value,
            ProfileId = result.Value.ProfileId.Value,
            FieldName = result.Value.FieldName,
            NewValue = result.Value.NewValue,
            ApprovedAt = result.Value.ApprovedAt
        };
    }

    /// <summary>
    /// Rejects a pending profile change request.
    /// Marks the request as rejected with the provided reason.
    /// Requires OWNER or ADMIN role (parent rejecting child's changes).
    /// </summary>
    [Authorize]
    [GraphQLDescription("Reject a pending profile change request. Requires Owner/Admin role.")]
    [UseMutationConvention]
    [Error<ValidationError>]
    [Error<BusinessError>]
    [Error<NotFoundError>]
    public async Task<RejectChangeResult> RejectChange(
        RejectProfileChangeInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RejectProfileChangeCommand(
            ChangeRequestId.From(input.RequestId),
            input.Reason);

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<RejectProfileChangeResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new BusinessException("REJECT_CHANGE_FAILED", result.Error);
        }

        return new RejectChangeResult
        {
            RequestId = result.Value.RequestId.Value,
            ProfileId = result.Value.ProfileId.Value,
            FieldName = result.Value.FieldName,
            Reason = result.Value.Reason,
            RejectedAt = result.Value.RejectedAt
        };
    }
}

/// <summary>
/// Result of updating a user profile.
/// </summary>
public sealed record UpdateProfileResult
{
    /// <summary>Profile unique identifier.</summary>
    public required Guid ProfileId { get; init; }

    /// <summary>Updated display name.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Timestamp of the update.</summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>Whether this was a newly created profile.</summary>
    public required bool IsNewProfile { get; init; }

    /// <summary>Whether the changes require parental approval (for child accounts).</summary>
    public required bool RequiresApproval { get; init; }

    /// <summary>Number of pending change requests for this profile.</summary>
    public required int PendingChangesCount { get; init; }
}

/// <summary>
/// Result of approving a profile change request.
/// </summary>
public sealed record ApproveChangeResult
{
    /// <summary>ID of the approved change request.</summary>
    public required Guid RequestId { get; init; }

    /// <summary>ID of the profile that was updated.</summary>
    public required Guid ProfileId { get; init; }

    /// <summary>Name of the field that was changed.</summary>
    public required string FieldName { get; init; }

    /// <summary>The new value that was applied.</summary>
    public required string NewValue { get; init; }

    /// <summary>Timestamp when the change was approved.</summary>
    public required DateTime ApprovedAt { get; init; }
}

/// <summary>
/// Result of rejecting a profile change request.
/// </summary>
public sealed record RejectChangeResult
{
    /// <summary>ID of the rejected change request.</summary>
    public required Guid RequestId { get; init; }

    /// <summary>ID of the profile associated with the request.</summary>
    public required Guid ProfileId { get; init; }

    /// <summary>Name of the field that was rejected.</summary>
    public required string FieldName { get; init; }

    /// <summary>Reason for rejection provided by the approver.</summary>
    public required string? Reason { get; init; }

    /// <summary>Timestamp when the change was rejected.</summary>
    public required DateTime RejectedAt { get; init; }
}
