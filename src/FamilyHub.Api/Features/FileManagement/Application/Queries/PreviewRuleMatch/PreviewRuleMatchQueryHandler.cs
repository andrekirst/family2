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
    : IQueryHandler<PreviewRuleMatchQuery, RuleMatchPreviewDto?>
{
    public async ValueTask<RuleMatchPreviewDto?> Handle(
        PreviewRuleMatchQuery query,
        CancellationToken cancellationToken)
    {
        var file = await fileRepository.GetByIdAsync(query.FileId, cancellationToken)
            ?? throw new DomainException("File not found", DomainErrorCodes.FileNotFound);

        if (file.FamilyId != query.FamilyId)
            throw new DomainException("File does not belong to this family", DomainErrorCodes.Forbidden);

        var rules = await ruleRepository.GetEnabledByFamilyIdAsync(query.FamilyId, cancellationToken);

        return ruleEngine.EvaluateFile(file, rules);
    }
}
