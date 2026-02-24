using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

public sealed record FolderMovedEvent(
    FolderId FolderId,
    FolderId? OldParentFolderId,
    FolderId NewParentFolderId,
    FamilyId FamilyId,
    UserId MovedBy,
    DateTime MovedAt) : DomainEvent;
