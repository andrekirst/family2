using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Events;

public sealed record ZipJobCompletedEvent(
    ZipJobId ZipJobId,
    int FileCount,
    long ZipSize,
    FamilyId FamilyId) : DomainEvent;
