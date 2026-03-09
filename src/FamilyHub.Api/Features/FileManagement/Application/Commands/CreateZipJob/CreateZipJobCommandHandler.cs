using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateZipJob;

public sealed class CreateZipJobCommandHandler(
    IZipJobRepository zipJobRepository,
    TimeProvider timeProvider)
    : ICommandHandler<CreateZipJobCommand, Result<CreateZipJobResult>>
{
    private const int MaxConcurrentJobs = 3;
    private const int MaxFilesPerZip = 1000;

    public async ValueTask<Result<CreateZipJobResult>> Handle(
        CreateZipJobCommand command,
        CancellationToken cancellationToken)
    {
        if (command.FileIds.Count > MaxFilesPerZip)
        {
            return DomainError.BusinessRule(
                DomainErrorCodes.ZipJobTooManyFiles,
                $"Cannot zip more than {MaxFilesPerZip} files at once");
        }

        var activeJobs = await zipJobRepository.GetActiveJobCountAsync(
            command.FamilyId, cancellationToken);

        if (activeJobs >= MaxConcurrentJobs)
        {
            return DomainError.BusinessRule(
                DomainErrorCodes.ZipJobConcurrentLimitReached,
                $"Maximum of {MaxConcurrentJobs} concurrent zip jobs per family");
        }

        var utcNow = timeProvider.GetUtcNow();
        var job = ZipJob.Create(command.FamilyId, command.UserId, command.FileIds, utcNow);
        await zipJobRepository.AddAsync(job, cancellationToken);

        return new CreateZipJobResult(job.Id.Value, job.Status.ToString());
    }
}
