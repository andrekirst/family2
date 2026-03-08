using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.PreviewRuleMatch;

public sealed record PreviewRuleMatchQuery(
    FileId FileId
) : IReadOnlyQuery<RuleMatchPreviewDto?>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
