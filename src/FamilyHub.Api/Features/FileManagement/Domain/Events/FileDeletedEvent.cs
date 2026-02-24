using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

/// <summary>
/// Raised when a file is removed from storage.
/// </summary>
public sealed record FileDeletedEvent(
    FileId FileId,
    FamilyId FamilyId,
    string StorageKey) : DomainEvent;
