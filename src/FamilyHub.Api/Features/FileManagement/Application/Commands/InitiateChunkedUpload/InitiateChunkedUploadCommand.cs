using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.InitiateChunkedUpload;

public sealed record InitiateChunkedUploadCommand
    : ICommand<Result<InitiateChunkedUploadResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
