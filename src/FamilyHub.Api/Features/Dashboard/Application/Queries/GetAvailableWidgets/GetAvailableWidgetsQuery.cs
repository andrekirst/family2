using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Dashboard.Models;

namespace FamilyHub.Api.Features.Dashboard.Application.Queries.GetAvailableWidgets;

public sealed record GetAvailableWidgetsQuery : IQuery<IReadOnlyList<WidgetDescriptorDto>>;
