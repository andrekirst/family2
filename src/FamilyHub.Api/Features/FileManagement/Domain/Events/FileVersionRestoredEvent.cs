using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

/// <summary>
/// Raised when a previous version is restored (creating a new current version).
/// </summary>
public sealed record FileVersionRestoredEvent(
    FileId FileId,
    FileVersionId RestoredFromVersionId,
    FileVersionId NewVersionId,
    int NewVersionNumber,
    UserId RestoredBy) : DomainEvent;
