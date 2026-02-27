using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Domain.Repositories;
using FamilyHub.Api.Features.Photos.Models;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Photos.Infrastructure.Repositories;

/// <summary>
/// Reads image files from the file_management.files table,
/// presenting them as photos in the Photos tab.
/// </summary>
public sealed class PhotoRepository(AppDbContext context) : IPhotoRepository
{
    /// <summary>
    /// Pre-filters to image MIME types at the SQL level.
    /// Using FromSqlRaw because Vogen value converters prevent
    /// LINQ translation of MimeType.Value.StartsWith().
    /// </summary>
    private IQueryable<StoredFile> ImageFiles() =>
        context.ManagedFiles.FromSqlRaw(
            "SELECT * FROM file_management.files WHERE mime_type LIKE 'image/%'");

    private static PhotoDto ToDto(StoredFile f) => new()
    {
        Id = f.Id.Value,
        FamilyId = f.FamilyId.Value,
        UploadedBy = f.UploadedBy.Value,
        FileName = f.Name.Value,
        ContentType = f.MimeType.Value,
        FileSizeBytes = f.Size.Value,
        StoragePath = "/api/files/" + f.StorageKey.Value + "/download",
        Caption = null,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt
    };

    public async Task<PhotoDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var fileId = FileId.From(id);
        var file = await ImageFiles()
            .FirstOrDefaultAsync(f => f.Id == fileId, ct);
        return file is not null ? ToDto(file) : null;
    }

    public async Task<List<PhotoDto>> GetByFamilyAsync(
        FamilyId familyId, int skip, int take, CancellationToken ct = default)
    {
        var files = await ImageFiles()
            .Where(f => f.FamilyId == familyId)
            .OrderByDescending(f => f.CreatedAt)
            .ThenByDescending(f => f.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return files.Select(ToDto).ToList();
    }

    public async Task<int> GetCountByFamilyAsync(FamilyId familyId, CancellationToken ct = default)
    {
        return await ImageFiles()
            .CountAsync(f => f.FamilyId == familyId, ct);
    }

    public async Task<PhotoDto?> GetNextAsync(
        FamilyId familyId, DateTime createdAt, Guid currentId, CancellationToken ct = default)
    {
        var file = await ImageFiles()
            .Where(f => f.FamilyId == familyId)
            .Where(f => f.CreatedAt < createdAt
                || (f.CreatedAt == createdAt && f.Id.Value.CompareTo(currentId) < 0))
            .OrderByDescending(f => f.CreatedAt)
            .ThenByDescending(f => f.Id)
            .FirstOrDefaultAsync(ct);
        return file is not null ? ToDto(file) : null;
    }

    public async Task<PhotoDto?> GetPreviousAsync(
        FamilyId familyId, DateTime createdAt, Guid currentId, CancellationToken ct = default)
    {
        var file = await ImageFiles()
            .Where(f => f.FamilyId == familyId)
            .Where(f => f.CreatedAt > createdAt
                || (f.CreatedAt == createdAt && f.Id.Value.CompareTo(currentId) > 0))
            .OrderBy(f => f.CreatedAt)
            .ThenBy(f => f.Id)
            .FirstOrDefaultAsync(ct);
        return file is not null ? ToDto(file) : null;
    }
}
