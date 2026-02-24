using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetTags;

public sealed class GetTagsQueryHandler(
    ITagRepository tagRepository,
    IFileTagRepository fileTagRepository)
    : IQueryHandler<GetTagsQuery, List<TagDto>>
{
    public async ValueTask<List<TagDto>> Handle(
        GetTagsQuery query,
        CancellationToken cancellationToken)
    {
        var tags = await tagRepository.GetByFamilyIdAsync(query.FamilyId, cancellationToken);

        var result = new List<TagDto>();
        foreach (var tag in tags)
        {
            var fileCount = await fileTagRepository.GetFileCountByTagIdAsync(tag.Id, cancellationToken);
            result.Add(FileManagementMapper.ToDto(tag, fileCount));
        }

        return result;
    }
}
