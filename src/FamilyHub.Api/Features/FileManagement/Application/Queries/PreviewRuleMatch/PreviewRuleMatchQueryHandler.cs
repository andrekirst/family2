using FamilyHub.Api.Features.FileManagement.Application.Services;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.PreviewRuleMatch;

public sealed class PreviewRuleMatchQueryHandler(
    IStoredFileRepository fileRepository,
    IOrganizationRuleRepository ruleRepository,
    IOrganizationRuleEngine ruleEngine)
    : IQueryHandler<PreviewRuleMatchQuery, Result<RuleMatchPreviewDto?>>
{
    public async ValueTask<Result<RuleMatchPreviewDto?>> Handle(
        PreviewRuleMatchQuery query,
        CancellationToken cancellationToken)
    {
        var file = await fileRepository.GetByIdAsync(query.FileId, cancellationToken);
        if (file is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found");
        }

        if (file.FamilyId != query.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "File does not belong to this family");
        }

        var rules = await ruleRepository.GetEnabledByFamilyIdAsync(query.FamilyId, cancellationToken);

        return ruleEngine.EvaluateFile(file, rules);
    }
}
