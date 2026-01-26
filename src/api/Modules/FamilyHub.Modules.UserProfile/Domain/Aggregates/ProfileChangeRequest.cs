using FamilyHub.Modules.UserProfile.Domain.Events;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Domain.Aggregates;

/// <summary>
/// ProfileChangeRequest aggregate root representing a pending profile change
/// that requires approval from a parent or admin.
/// </summary>
/// <remarks>
/// When a user with the "Child" family role attempts to modify their profile,
/// the changes are captured in a ProfileChangeRequest instead of being applied directly.
/// A parent (Owner or Admin) must then approve or reject the change.
/// </remarks>
public class ProfileChangeRequest : AggregateRoot<ChangeRequestId>
{
    /// <summary>
    /// The profile being modified.
    /// </summary>
    public UserProfileId ProfileId { get; private set; }

    /// <summary>
    /// The user who requested the change (child user).
    /// </summary>
    public UserId RequestedBy { get; private set; }

    /// <summary>
    /// The family ID for routing to the correct approvers.
    /// </summary>
    public FamilyId FamilyId { get; private set; }

    /// <summary>
    /// The name of the field being changed (e.g., "DisplayName", "Birthday").
    /// </summary>
    public string FieldName { get; private set; } = string.Empty;

    /// <summary>
    /// The current value of the field (null if not previously set).
    /// Stored as string for flexibility across different field types.
    /// </summary>
    public string? OldValue { get; private set; }

    /// <summary>
    /// The requested new value for the field.
    /// Stored as string for flexibility across different field types.
    /// </summary>
    public string NewValue { get; private set; } = string.Empty;

    /// <summary>
    /// Current status of the change request.
    /// </summary>
    public ChangeRequestStatus Status { get; private set; }

    /// <summary>
    /// The user who reviewed (approved/rejected) the request.
    /// Null while status is Pending.
    /// </summary>
    public UserId? ReviewedBy { get; private set; }

    /// <summary>
    /// Timestamp when the request was reviewed.
    /// Null while status is Pending.
    /// </summary>
    public DateTime? ReviewedAt { get; private set; }

    /// <summary>
    /// Reason for rejection if the request was rejected.
    /// Null if approved or still pending.
    /// </summary>
    public string? RejectionReason { get; private set; }

    /// <summary>
    /// Private constructor for EF Core materialization.
    /// </summary>
    private ProfileChangeRequest() : base(ChangeRequestId.From(Guid.Empty))
    {
        ProfileId = UserProfileId.From(Guid.Empty);
        RequestedBy = UserId.From(Guid.Empty);
        FamilyId = FamilyId.From(Guid.Empty);
        Status = ChangeRequestStatus.Pending;
    }

    private ProfileChangeRequest(
        ChangeRequestId id,
        UserProfileId profileId,
        UserId requestedBy,
        FamilyId familyId,
        string fieldName,
        string? oldValue,
        string newValue) : base(id)
    {
        ProfileId = profileId;
        RequestedBy = requestedBy;
        FamilyId = familyId;
        FieldName = fieldName;
        OldValue = oldValue;
        NewValue = newValue;
        Status = ChangeRequestStatus.Pending;
    }

    /// <summary>
    /// Creates a new profile change request.
    /// </summary>
    /// <param name="profileId">The profile being modified.</param>
    /// <param name="requestedBy">The user requesting the change.</param>
    /// <param name="familyId">The family ID for routing to approvers.</param>
    /// <param name="fieldName">The name of the field being changed.</param>
    /// <param name="oldValue">The current value of the field.</param>
    /// <param name="newValue">The requested new value.</param>
    /// <returns>A new ProfileChangeRequest instance with Pending status.</returns>
    public static ProfileChangeRequest Create(
        UserProfileId profileId,
        UserId requestedBy,
        FamilyId familyId,
        string fieldName,
        string? oldValue,
        string newValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        ArgumentException.ThrowIfNullOrWhiteSpace(newValue);

        var request = new ProfileChangeRequest(
            ChangeRequestId.New(),
            profileId,
            requestedBy,
            familyId,
            fieldName,
            oldValue,
            newValue);

        request.AddDomainEvent(new ProfileChangeRequestedEvent(
            eventVersion: 1,
            requestId: request.Id,
            profileId: profileId,
            requestedBy: requestedBy,
            familyId: familyId,
            fieldName: fieldName,
            oldValue: oldValue,
            newValue: newValue));

        return request;
    }

    /// <summary>
    /// Approves the change request. The change should then be applied to the profile.
    /// </summary>
    /// <param name="approvedBy">The user approving the request (must be Owner or Admin).</param>
    /// <exception cref="InvalidOperationException">If the request is not in Pending status.</exception>
    public void Approve(UserId approvedBy)
    {
        if (Status != ChangeRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot approve a change request with status '{Status.Value}'. Only pending requests can be approved.");
        }

        Status = ChangeRequestStatus.Approved;
        ReviewedBy = approvedBy;
        ReviewedAt = DateTime.UtcNow;

        AddDomainEvent(new ProfileChangeApprovedEvent(
            eventVersion: 1,
            requestId: Id,
            profileId: ProfileId,
            approvedBy: approvedBy,
            fieldName: FieldName,
            newValue: NewValue));
    }

    /// <summary>
    /// Rejects the change request with a reason.
    /// </summary>
    /// <param name="rejectedBy">The user rejecting the request (must be Owner or Admin).</param>
    /// <param name="reason">The reason for rejection (required, minimum 10 characters).</param>
    /// <exception cref="InvalidOperationException">If the request is not in Pending status.</exception>
    /// <exception cref="ArgumentException">If the reason is empty or too short.</exception>
    public void Reject(UserId rejectedBy, string reason)
    {
        if (Status != ChangeRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot reject a change request with status '{Status.Value}'. Only pending requests can be rejected.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (reason.Length < 10)
        {
            throw new ArgumentException(
                "Rejection reason must be at least 10 characters long to provide meaningful feedback.",
                nameof(reason));
        }

        Status = ChangeRequestStatus.Rejected;
        ReviewedBy = rejectedBy;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = reason;

        AddDomainEvent(new ProfileChangeRejectedEvent(
            eventVersion: 1,
            requestId: Id,
            profileId: ProfileId,
            rejectedBy: rejectedBy,
            fieldName: FieldName,
            reason: reason));
    }

    /// <summary>
    /// Returns true if this request is still pending approval.
    /// </summary>
    public bool IsPending => Status == ChangeRequestStatus.Pending;

    /// <summary>
    /// Returns true if this request has been approved.
    /// </summary>
    public bool IsApproved => Status == ChangeRequestStatus.Approved;

    /// <summary>
    /// Returns true if this request has been rejected.
    /// </summary>
    public bool IsRejected => Status == ChangeRequestStatus.Rejected;
}
