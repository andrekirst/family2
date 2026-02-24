using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

public sealed record FileMovedEvent(
    FileId FileId,
    FolderId FromFolderId,
    FolderId ToFolderId,
    FamilyId FamilyId,
    UserId MovedBy,
    DateTime MovedAt) : DomainEvent;
