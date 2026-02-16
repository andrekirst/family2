using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Models;

namespace FamilyHub.Api.Features.Dashboard.Application.Queries.GetMyDashboard;

public sealed record GetMyDashboardQuery(
    UserId UserId
) : IQuery<DashboardLayoutDto?>;
