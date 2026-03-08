using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.ResetDashboard;

public sealed record ResetDashboardCommand(
    DashboardId DashboardId,
    FamilyId FamilyId
) : ICommand<bool>, IFamilyScoped;
