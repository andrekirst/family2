using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Photos.Domain.Events;

public sealed record PhotoUploadedEvent(
    PhotoId PhotoId,
    FamilyId FamilyId,
    UserId UploadedBy,
    string FileName,
    DateTime CreatedAt
) : DomainEvent;
