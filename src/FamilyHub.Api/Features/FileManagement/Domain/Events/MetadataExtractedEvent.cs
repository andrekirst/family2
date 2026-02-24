using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

public sealed record MetadataExtractedEvent(
    FileId FileId,
    double? GpsLatitude,
    double? GpsLongitude,
    string? CameraModel,
    DateTime? CaptureDate) : DomainEvent;
