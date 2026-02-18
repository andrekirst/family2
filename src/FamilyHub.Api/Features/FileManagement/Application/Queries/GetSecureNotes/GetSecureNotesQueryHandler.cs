using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetSecureNotes;

public sealed class GetSecureNotesQueryHandler(
    ISecureNoteRepository noteRepository)
    : IQueryHandler<GetSecureNotesQuery, List<SecureNoteDto>>
{
    public async ValueTask<List<SecureNoteDto>> Handle(
        GetSecureNotesQuery query,
        CancellationToken cancellationToken)
    {
        var notes = query.Category.HasValue
            ? await noteRepository.GetByUserIdAndCategoryAsync(
                query.UserId, query.FamilyId, query.Category.Value, cancellationToken)
            : await noteRepository.GetByUserIdAsync(
                query.UserId, query.FamilyId, cancellationToken);

        return notes.Select(FileManagementMapper.ToDto).ToList();
    }
}
