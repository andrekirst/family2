using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Features.FileManagement.Application.Services;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Services;
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

        // Application services
        services.AddSingleton<IMetadataExtractionService, MetadataExtractionService>();
        services.AddScoped<IFileManagementAuthorizationService, FileManagementAuthorizationService>();

        // Domain repositories
        services.AddScoped<IStoredFileRepository, StoredFileRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IFileTagRepository, FileTagRepository>();
        services.AddScoped<IFileMetadataRepository, FileMetadataRepository>();
        services.AddScoped<IAlbumRepository, AlbumRepository>();
        services.AddScoped<IAlbumItemRepository, AlbumItemRepository>();
        services.AddScoped<IUserFavoriteRepository, UserFavoriteRepository>();
        services.AddScoped<IFilePermissionRepository, FilePermissionRepository>();
        services.AddScoped<IRecentSearchRepository, RecentSearchRepository>();
        services.AddScoped<ISavedSearchRepository, SavedSearchRepository>();
        services.AddScoped<IFileSearchService, FileSearchService>();
        services.AddScoped<IOrganizationRuleRepository, OrganizationRuleRepository>();
        services.AddScoped<IProcessingLogRepository, ProcessingLogRepository>();
        services.AddSingleton<IOrganizationRuleEngine, OrganizationRuleEngine>();
        services.AddScoped<IFileVersionRepository, FileVersionRepository>();
    }
}
