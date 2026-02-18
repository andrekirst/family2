using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.PreviewRuleMatch;

public sealed record PreviewRuleMatchQuery(
    FileId FileId,
    FamilyId FamilyId
) : IQuery<RuleMatchPreviewDto?>;
