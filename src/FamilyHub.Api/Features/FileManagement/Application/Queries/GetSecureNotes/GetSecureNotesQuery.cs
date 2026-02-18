using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetSecureNotes;

public sealed record GetSecureNotesQuery(
    UserId UserId,
    FamilyId FamilyId,
    NoteCategory? Category
) : IQuery<List<SecureNoteDto>>;
