using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

/// <summary>
/// Raised when a file is successfully stored in the storage backend.
/// </summary>
public sealed record FileUploadedEvent(
    FileId FileId,
    FamilyId FamilyId,
    string StorageKey,
    MimeType MimeType,
    FileSize Size,
    Checksum Checksum) : DomainEvent;
