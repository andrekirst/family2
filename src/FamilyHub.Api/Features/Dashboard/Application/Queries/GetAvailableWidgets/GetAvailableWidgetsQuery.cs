using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Dashboard.Models;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Dashboard.Application.Queries.GetAvailableWidgets;

public sealed record GetAvailableWidgetsQuery : IReadOnlyQuery<IReadOnlyList<WidgetDescriptorDto>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
