using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Dashboard.Models;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Dashboard.Application.Queries.GetAvailableWidgets;

public sealed record GetAvailableWidgetsQuery(
    FamilyId FamilyId
) : IReadOnlyQuery<IReadOnlyList<WidgetDescriptorDto>>, IFamilyScoped;
