using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

/// <summary>
/// Raised when a new version of a file is created (upload, re-upload, or restore).
/// </summary>
public sealed record FileVersionCreatedEvent(
    FileId FileId,
    FileVersionId VersionId,
    int VersionNumber,
    UserId UploadedBy,
    FileSize FileSize) : DomainEvent;
