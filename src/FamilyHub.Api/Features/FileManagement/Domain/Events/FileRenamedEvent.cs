using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

public sealed record FileRenamedEvent(
    FileId FileId,
    FileName OldName,
    FileName NewName,
    FamilyId FamilyId,
    UserId RenamedBy,
    DateTime RenamedAt) : DomainEvent;
