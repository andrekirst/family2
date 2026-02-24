using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

public sealed record FolderCreatedEvent(
    FolderId FolderId,
    FileName FolderName,
    FolderId? ParentFolderId,
    FamilyId FamilyId,
    UserId CreatedBy,
    DateTime CreatedAt) : DomainEvent;
