using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetShareLinkAccessLog;

public sealed record GetShareLinkAccessLogQuery(
    ShareLinkId ShareLinkId,
    FamilyId FamilyId
) : IQuery<List<ShareLinkAccessLogDto>>;
