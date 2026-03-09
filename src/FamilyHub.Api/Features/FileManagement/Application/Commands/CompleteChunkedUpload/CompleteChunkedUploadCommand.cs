using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CompleteChunkedUpload;

public sealed record CompleteChunkedUploadCommand(
    string UploadId,
    string FileName
) : ICommand<Result<CompleteChunkedUploadResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
