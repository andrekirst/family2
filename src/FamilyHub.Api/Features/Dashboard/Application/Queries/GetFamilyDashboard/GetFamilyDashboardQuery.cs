using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Models;

namespace FamilyHub.Api.Features.Dashboard.Application.Queries.GetFamilyDashboard;

public sealed record GetFamilyDashboardQuery(
    FamilyId FamilyId
) : IReadOnlyQuery<DashboardLayoutDto?>, IFamilyScoped;
