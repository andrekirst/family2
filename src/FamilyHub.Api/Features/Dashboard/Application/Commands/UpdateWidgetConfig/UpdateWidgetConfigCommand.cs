using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FamilyHub.Api.Features.Dashboard.Models;

namespace FamilyHub.Api.Features.Dashboard.Application.Commands.UpdateWidgetConfig;

public sealed record UpdateWidgetConfigCommand(
    DashboardWidgetId WidgetId,
    string? ConfigJson,
    FamilyId FamilyId
) : ICommand<DashboardWidgetDto>, IFamilyScoped;
