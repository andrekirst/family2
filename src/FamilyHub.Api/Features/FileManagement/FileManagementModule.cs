using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.FileManagement.Application.Commands.ProcessInboxFiles;
using FamilyHub.Api.Features.FileManagement.Application.Search;
using FamilyHub.Api.Features.FileManagement.Application.Services;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Endpoints;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Services;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using Minio;

namespace FamilyHub.Api.Features.FileManagement;

/// <summary>
/// File Management module registration.
/// Registers storage infrastructure, services, and repositories for the file management domain.
/// </summary>
[ModuleOrder(600)]
public sealed class FileManagementModule : IModule, IEndpointModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Storage configuration
        services.Configure<StorageQuotaOptions>(
            configuration.GetSection(StorageQuotaOptions.SectionName));

        // Storage infrastructure — conditional provider based on configuration
        var provider = configuration.GetValue<string>("FileManagement:Storage:Provider") ?? "Postgres";

        if (provider.Equals("MinIO", StringComparison.OrdinalIgnoreCase))
        {
            services.Configure<MinioStorageOptions>(
                configuration.GetSection(MinioStorageOptions.SectionName));

            var minioOptions = configuration.GetSection(MinioStorageOptions.SectionName)
                .Get<MinioStorageOptions>() ?? new MinioStorageOptions();

            services.AddSingleton<IMinioClient>(_ =>
                new MinioClient()
                    .WithEndpoint(minioOptions.Endpoint)
                    .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey)
                    .WithSSL(minioOptions.UseSSL)
                    .Build());

            services.AddSingleton<IStorageProvider, MinioStorageProvider>();
            services.AddHostedService<MinioBucketInitializer>();
        }
        else
        {
            services.AddScoped<IStorageProvider, PostgresStorageProvider>();
        }

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
        services.AddScoped<IInboxFileProcessor, InboxFileProcessor>();
        services.AddScoped<IFileVersionRepository, FileVersionRepository>();
        services.AddScoped<IShareLinkRepository, ShareLinkRepository>();
        services.AddScoped<IShareLinkAccessLogRepository, ShareLinkAccessLogRepository>();
        services.AddScoped<IFileThumbnailRepository, FileThumbnailRepository>();
        services.AddSingleton<IThumbnailGenerationService, ThumbnailGenerationService>();
        services.AddScoped<ISecureNoteRepository, SecureNoteRepository>();
        services.AddScoped<IExternalConnectionRepository, ExternalConnectionRepository>();
        services.AddScoped<IZipJobRepository, ZipJobRepository>();

        // Search
        services.AddScoped<ISearchProvider, FileManagementSearchProvider>();
        services.AddSingleton<ICommandPaletteProvider, FileManagementCommandPaletteProvider>();
    }

    public void MapEndpoints(WebApplication app)
    {
        app.MapFileEndpoints();
    }
}
