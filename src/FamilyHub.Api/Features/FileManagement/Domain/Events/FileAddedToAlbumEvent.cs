using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

public sealed record FileAddedToAlbumEvent(
    FileId FileId,
    AlbumId AlbumId,
    FamilyId FamilyId) : DomainEvent;
