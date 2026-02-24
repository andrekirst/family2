using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetProcessingLog;

public sealed record GetProcessingLogQuery(
    FamilyId FamilyId,
    int Skip = 0,
    int Take = 50
) : IQuery<List<ProcessingLogEntryDto>>;
