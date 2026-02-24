using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetShareLinks;

public sealed record GetShareLinksQuery(
    FamilyId FamilyId
) : IQuery<List<ShareLinkDto>>;
