using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.ResetDashboard;

public sealed record ResetDashboardCommand(
    DashboardId DashboardId
) : ICommand<bool>;
