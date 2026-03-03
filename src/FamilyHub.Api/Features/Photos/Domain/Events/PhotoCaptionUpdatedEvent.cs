using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Photos.Domain.Events;

public sealed record PhotoCaptionUpdatedEvent(
    PhotoId PhotoId,
    PhotoCaption NewCaption,
    DateTime UpdatedAt
) : DomainEvent;
