using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetOrganizationRules;

public sealed class GetOrganizationRulesQueryHandler(
    IOrganizationRuleRepository ruleRepository)
    : IQueryHandler<GetOrganizationRulesQuery, List<OrganizationRuleDto>>
{
    public async ValueTask<List<OrganizationRuleDto>> Handle(
        GetOrganizationRulesQuery query,
        CancellationToken cancellationToken)
    {
        var rules = await ruleRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);

        return rules.Select(FileManagementMapper.ToDto).ToList();
    }
}
