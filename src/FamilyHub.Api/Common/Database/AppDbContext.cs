using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.Avatar;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Data;
using FamilyHub.EventChain.Domain.Entities;
using FileManagementStoredFile = FamilyHub.Api.Features.FileManagement.Domain.Entities.StoredFile;
using FileManagementFolder = FamilyHub.Api.Features.FileManagement.Domain.Entities.Folder;
using FileManagementTag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;
using FileManagementFileTag = FamilyHub.Api.Features.FileManagement.Domain.Entities.FileTag;
using FileManagementFileMetadata = FamilyHub.Api.Features.FileManagement.Domain.Entities.FileMetadata;
using FileManagementAlbum = FamilyHub.Api.Features.FileManagement.Domain.Entities.Album;
using FileManagementAlbumItem = FamilyHub.Api.Features.FileManagement.Domain.Entities.AlbumItem;
using FileManagementUserFavorite = FamilyHub.Api.Features.FileManagement.Domain.Entities.UserFavorite;
using FileManagementFilePermission = FamilyHub.Api.Features.FileManagement.Domain.Entities.FilePermission;
using FileManagementRecentSearch = FamilyHub.Api.Features.FileManagement.Domain.Entities.RecentSearch;
using FileManagementSavedSearch = FamilyHub.Api.Features.FileManagement.Domain.Entities.SavedSearch;
using FileManagementOrganizationRule = FamilyHub.Api.Features.FileManagement.Domain.Entities.OrganizationRule;
using FileManagementProcessingLogEntry = FamilyHub.Api.Features.FileManagement.Domain.Entities.ProcessingLogEntry;
using FileManagementFileVersion = FamilyHub.Api.Features.FileManagement.Domain.Entities.FileVersion;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Common.Database;

/// <summary>
/// Application database context for Family Hub.
/// Uses PostgreSQL with schema separation for organization.
/// Implements IUnitOfWork for the TransactionBehavior pipeline.
/// Domain events are collected by DomainEventInterceptor and published
/// by DomainEventPublishingBehavior — no longer handled here.
/// </summary>
public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Users authenticated via OAuth (Keycloak)
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Family households
    /// </summary>
    public DbSet<Family> Families { get; set; }

    /// <summary>
    /// Family membership records with roles
    /// </summary>
    public DbSet<FamilyMember> FamilyMembers { get; set; }

    /// <summary>
    /// Family invitations with lifecycle tracking
    /// </summary>
    public DbSet<FamilyInvitation> FamilyInvitations { get; set; }

    /// <summary>
    /// Calendar events
    /// </summary>
    public DbSet<CalendarEvent> CalendarEvents { get; set; }

    /// <summary>
    /// Calendar event attendees (join table)
    /// </summary>
    public DbSet<CalendarEventAttendee> CalendarEventAttendees { get; set; }

    // Avatar infrastructure entities
    public DbSet<AvatarAggregate> Avatars { get; set; }
    public DbSet<AvatarVariant> AvatarVariants { get; set; }
    public DbSet<StoredFile> StoredFiles { get; set; }

    // Dashboard entities
    public DbSet<DashboardLayout> DashboardLayouts { get; set; }
    public DbSet<DashboardWidget> DashboardWidgets { get; set; }

    // File Management entities
    public DbSet<FileBlob> FileBlobs { get; set; }
    public DbSet<StorageQuota> StorageQuotas { get; set; }
    public DbSet<UploadChunk> UploadChunks { get; set; }

    // File Management domain entities
    public DbSet<FileManagementStoredFile> ManagedFiles { get; set; }
    public DbSet<FileManagementFolder> Folders { get; set; }
    public DbSet<FileManagementTag> Tags { get; set; }
    public DbSet<FileManagementFileTag> FileTags { get; set; }
    public DbSet<FileManagementFileMetadata> FileMetadatas { get; set; }
    public DbSet<FileManagementAlbum> Albums { get; set; }
    public DbSet<FileManagementAlbumItem> AlbumItems { get; set; }
    public DbSet<FileManagementUserFavorite> UserFavorites { get; set; }
    public DbSet<FileManagementFilePermission> FilePermissions { get; set; }
    public DbSet<FileManagementRecentSearch> RecentSearches { get; set; }
    public DbSet<FileManagementSavedSearch> SavedSearches { get; set; }
    public DbSet<FileManagementOrganizationRule> OrganizationRules { get; set; }
    public DbSet<FileManagementProcessingLogEntry> ProcessingLogEntries { get; set; }
    public DbSet<FileManagementFileVersion> FileVersions { get; set; }

    // Event Chain Engine entities
    public DbSet<ChainDefinition> ChainDefinitions { get; set; }
    public DbSet<ChainDefinitionStep> ChainDefinitionSteps { get; set; }
    public DbSet<ChainExecution> ChainExecutions { get; set; }
    public DbSet<StepExecution> StepExecutions { get; set; }
    public DbSet<ChainEntityMapping> ChainEntityMappings { get; set; }
    public DbSet<ChainScheduledJob> ChainScheduledJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensure PostgreSQL schemas exist
        modelBuilder.HasDefaultSchema("public");

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// Override SaveChanges to automatically update UpdatedAt timestamps.
    /// Note: Aggregates (User, Family) manage their own timestamps via domain methods.
    /// </summary>
    public override int SaveChanges()
    {
        UpdateNonAggregateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update UpdatedAt timestamps.
    /// Domain events are now collected by DomainEventInterceptor (via IHasDomainEvents)
    /// and published by DomainEventPublishingBehavior in the Mediator pipeline.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateNonAggregateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Update timestamps for non-aggregate entities.
    /// Aggregates manage their own timestamps via domain methods.
    /// </summary>
    private void UpdateNonAggregateTimestamps()
    {
        // Currently no non-aggregate entities with timestamps
        // This method reserved for future use
        // Note: User and Family (aggregates) manage their own UpdatedAt via domain methods
    }
}
