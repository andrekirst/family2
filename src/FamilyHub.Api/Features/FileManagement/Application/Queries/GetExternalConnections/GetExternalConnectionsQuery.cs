using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetExternalConnections;

public sealed record GetExternalConnectionsQuery(
    FamilyId FamilyId
) : IReadOnlyQuery<List<ExternalConnectionDto>>, IFamilyScoped;
