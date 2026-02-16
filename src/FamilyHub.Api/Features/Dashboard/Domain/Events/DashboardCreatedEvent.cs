using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Dashboard.Domain.Events;

public sealed record DashboardCreatedEvent(
    DashboardId DashboardId,
    UserId CreatedByUserId,
    bool IsShared) : DomainEvent;
