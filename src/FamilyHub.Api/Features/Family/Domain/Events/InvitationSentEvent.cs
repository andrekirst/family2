using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Events;

/// <summary>
/// Domain event raised when a family invitation is sent.
/// Triggers the email sending event handler.
/// Contains the plaintext token needed to build the invitation URL.
/// </summary>
public sealed record InvitationSentEvent(
    InvitationId InvitationId,
    FamilyId FamilyId,
    UserId InvitedByUserId,
    Email InviteeEmail,
    FamilyRole Role,
    string PlaintextToken,
    DateTime ExpiresAt
) : DomainEvent;
