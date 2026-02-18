using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

namespace FamilyHub.Api.Features.FileManagement;

/// <summary>
/// File Management module registration.
/// Registers storage infrastructure, services, and repositories for the file management domain.
/// </summary>
public sealed class FileManagementModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Storage configuration
        services.Configure<StorageQuotaOptions>(
            configuration.GetSection(StorageQuotaOptions.SectionName));

        // Storage infrastructure
        services.AddScoped<IStorageProvider, PostgresStorageProvider>();
        services.AddSingleton<IMimeDetector, MimeDetector>();
        services.AddSingleton<IChecksumCalculator, ChecksumCalculator>();
        services.AddScoped<IStorageQuotaService, StorageQuotaService>();
        services.AddScoped<IFileManagementStorageService, FileManagementStorageService>();

        // Domain repositories
        services.AddScoped<IStoredFileRepository, StoredFileRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
    }
}
