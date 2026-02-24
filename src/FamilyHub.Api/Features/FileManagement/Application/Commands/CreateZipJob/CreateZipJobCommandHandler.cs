using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateZipJob;

public sealed class CreateZipJobCommandHandler(
    IZipJobRepository zipJobRepository)
    : ICommandHandler<CreateZipJobCommand, CreateZipJobResult>
{
    private const int MaxConcurrentJobs = 3;
    private const int MaxFilesPerZip = 1000;

    public async ValueTask<CreateZipJobResult> Handle(
        CreateZipJobCommand command,
        CancellationToken cancellationToken)
    {
        if (command.FileIds.Count > MaxFilesPerZip)
            throw new DomainException(
                $"Cannot zip more than {MaxFilesPerZip} files at once",
                DomainErrorCodes.ZipJobTooManyFiles);

        var activeJobs = await zipJobRepository.GetActiveJobCountAsync(
            command.FamilyId, cancellationToken);

        if (activeJobs >= MaxConcurrentJobs)
            throw new DomainException(
                $"Maximum of {MaxConcurrentJobs} concurrent zip jobs per family",
                DomainErrorCodes.ZipJobConcurrentLimitReached);

        var job = ZipJob.Create(command.FamilyId, command.InitiatedBy, command.FileIds);
        await zipJobRepository.AddAsync(job, cancellationToken);

        return new CreateZipJobResult(job.Id.Value, job.Status.ToString());
    }
}
