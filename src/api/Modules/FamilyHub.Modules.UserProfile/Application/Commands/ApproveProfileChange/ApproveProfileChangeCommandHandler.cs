using System.Text.Json;
using FamilyHub.Modules.UserProfile.Application.Services.EventSourcing;
using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.Modules.UserProfile.Persistence;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using DomainResult = FamilyHub.SharedKernel.Domain.Result<FamilyHub.Modules.UserProfile.Application.Commands.ApproveProfileChange.ApproveProfileChangeResult>;

namespace FamilyHub.Modules.UserProfile.Application.Commands.ApproveProfileChange;

/// <summary>
/// Handler for ApproveProfileChangeCommand.
/// Applies the approved change to the profile and marks the request as approved.
/// </summary>
public sealed partial class ApproveProfileChangeCommandHandler(
    IUserContext userContext,
    IProfileChangeRequestRepository changeRequestRepository,
    IUserProfileRepository profileRepository,
    UserProfileDbContext dbContext,
    IProfileEventRecorder eventRecorder,
    ILogger<ApproveProfileChangeCommandHandler> logger)
    : ICommandHandler<ApproveProfileChangeCommand, DomainResult>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <inheritdoc />
    public async Task<DomainResult> Handle(
        ApproveProfileChangeCommand request,
        CancellationToken cancellationToken)
    {
        var approverId = userContext.UserId;
        LogApprovingChangeRequest(request.RequestId.Value, approverId.Value);

        // Load the change request
        var changeRequest = await changeRequestRepository.GetByIdAsync(request.RequestId, cancellationToken);

        if (changeRequest == null)
        {
            LogChangeRequestNotFound(request.RequestId.Value);
            return Result.Failure<ApproveProfileChangeResult>("Change request not found.");
        }

        // Verify the request is still pending
        if (!changeRequest.IsPending)
        {
            LogChangeRequestNotPending(request.RequestId.Value, changeRequest.Status.Value);
            return Result.Failure<ApproveProfileChangeResult>(
                $"Change request is not pending. Current status: {changeRequest.Status.Value}");
        }

        // Load the target profile
        var profile = await profileRepository.GetByIdAsync(changeRequest.ProfileId, cancellationToken);

        if (profile == null)
        {
            LogProfileNotFound(changeRequest.ProfileId.Value);
            return Result.Failure<ApproveProfileChangeResult>("Profile not found.");
        }

        // Apply the change to the profile
        ApplyChangeToProfile(profile, changeRequest);

        // Mark the request as approved (this raises ProfileChangeApprovedEvent)
        changeRequest.Approve(approverId);

        // Record the field update event for audit trail
        await eventRecorder.RecordFieldUpdateAsync(
            profile.Id,
            changeRequest.RequestedBy, // Original requester is the one who made the change
            changeRequest.FieldName,
            changeRequest.OldValue,
            changeRequest.NewValue,
            cancellationToken);

        // Persist atomically
        await dbContext.SaveChangesAsync(cancellationToken);

        LogChangeRequestApproved(request.RequestId.Value, changeRequest.FieldName);

        return DomainResult.Success(new ApproveProfileChangeResult
        {
            RequestId = changeRequest.Id,
            ProfileId = profile.Id,
            FieldName = changeRequest.FieldName,
            NewValue = changeRequest.NewValue,
            ApprovedAt = changeRequest.ReviewedAt!.Value
        });
    }

    /// <summary>
    /// Applies the approved change to the profile based on the field name.
    /// </summary>
    private void ApplyChangeToProfile(
        Domain.Aggregates.UserProfile profile,
        Domain.Aggregates.ProfileChangeRequest changeRequest)
    {
        switch (changeRequest.FieldName)
        {
            case nameof(ProfileStateDto.DisplayName):
                profile.UpdateDisplayName(DisplayName.From(changeRequest.NewValue));
                break;

            case nameof(ProfileStateDto.Birthday):
                if (DateOnly.TryParse(changeRequest.NewValue, out var birthday))
                {
                    profile.UpdateBirthday(Birthday.From(birthday));
                }
                break;

            case nameof(ProfileStateDto.Pronouns):
                profile.UpdatePronouns(Pronouns.From(changeRequest.NewValue));
                break;

            case "Preferences":
                var preferences = DeserializePreferences(changeRequest.NewValue);
                if (preferences != null)
                {
                    profile.UpdatePreferences(preferences);
                }
                break;

            case "FieldVisibility":
                var visibility = DeserializeFieldVisibility(changeRequest.NewValue);
                if (visibility != null)
                {
                    profile.UpdateFieldVisibility(visibility);
                }
                break;

            default:
                LogUnknownFieldName(changeRequest.FieldName);
                break;
        }
    }

    private static ProfilePreferences? DeserializePreferences(string json)
    {
        try
        {
            var data = JsonSerializer.Deserialize<PreferencesDto>(json, JsonOptions);
            if (data == null) return null;

            return ProfilePreferences.Create(
                data.Language,
                data.Timezone,
                data.DateFormat);
        }
        catch
        {
            return null;
        }
    }

    private static ProfileFieldVisibility? DeserializeFieldVisibility(string json)
    {
        try
        {
            var data = JsonSerializer.Deserialize<FieldVisibilityDto>(json, JsonOptions);
            if (data == null) return null;

            return ProfileFieldVisibility.Create(
                VisibilityLevel.From(data.BirthdayVisibility ?? "family"),
                VisibilityLevel.From(data.PronounsVisibility ?? "family"),
                VisibilityLevel.From(data.PreferencesVisibility ?? "hidden"));
        }
        catch
        {
            return null;
        }
    }

    private sealed class PreferencesDto
    {
        public string? Language { get; set; }
        public string? Timezone { get; set; }
        public string? DateFormat { get; set; }
    }

    private sealed class FieldVisibilityDto
    {
        public string? BirthdayVisibility { get; set; }
        public string? PronounsVisibility { get; set; }
        public string? PreferencesVisibility { get; set; }
    }

    [LoggerMessage(LogLevel.Information, "Approving change request {requestId} by user {approverId}")]
    partial void LogApprovingChangeRequest(Guid requestId, Guid approverId);

    [LoggerMessage(LogLevel.Warning, "Change request {requestId} not found")]
    partial void LogChangeRequestNotFound(Guid requestId);

    [LoggerMessage(LogLevel.Warning, "Change request {requestId} is not pending, status: {status}")]
    partial void LogChangeRequestNotPending(Guid requestId, string status);

    [LoggerMessage(LogLevel.Warning, "Profile {profileId} not found for change request")]
    partial void LogProfileNotFound(Guid profileId);

    [LoggerMessage(LogLevel.Information, "Change request {requestId} approved for field {fieldName}")]
    partial void LogChangeRequestApproved(Guid requestId, string fieldName);

    [LoggerMessage(LogLevel.Warning, "Unknown field name in change request: {fieldName}")]
    partial void LogUnknownFieldName(string fieldName);
}
