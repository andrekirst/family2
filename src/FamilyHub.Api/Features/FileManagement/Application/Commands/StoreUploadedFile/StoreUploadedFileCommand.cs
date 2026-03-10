using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.StoreUploadedFile;

/// <summary>
/// Command to store an uploaded file. The endpoint reads the IFormFile bytes into a MemoryStream
/// and passes it here. The handler calls IFileManagementStorageService (which needs FamilyId for
/// quota enforcement) and returns the storage result metadata.
/// </summary>
public sealed record StoreUploadedFileCommand(
    Stream FileStream,
    string FileName
) : ICommand<Result<StoreUploadedFileResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
