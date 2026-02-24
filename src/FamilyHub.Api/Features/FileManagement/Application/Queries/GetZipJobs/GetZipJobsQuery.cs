using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetZipJobs;

public sealed record GetZipJobsQuery(
    FamilyId FamilyId
) : IQuery<List<ZipJobDto>>;
