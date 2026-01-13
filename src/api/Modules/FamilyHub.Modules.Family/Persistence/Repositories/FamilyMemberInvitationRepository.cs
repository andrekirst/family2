using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Family.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the FamilyMemberInvitation repository using FamilyDbContext.
///
/// PHASE 5 STATE: Repository now resides in Family module with its own DbContext.
///
/// CROSS-MODULE QUERIES:
/// - IsUserMemberOfFamilyAsync requires data from Auth module (User table)
/// - This query is handled via IUserLookupService abstraction
/// - This maintains bounded context separation while enabling necessary cross-module operations
///
/// SPECIFICATION PATTERN:
/// - Implements ISpecificationRepository for specification-based queries
/// - Use FindOneAsync/FindAllAsync with specifications for new code
/// - Legacy methods are marked [Obsolete] for gradual migration
/// </summary>
/// <param name="context">The Family module database context.</param>
/// <param name="userLookupService">Service for cross-module user lookups.</param>
public sealed class FamilyMemberInvitationRepository(FamilyDbContext context, IUserLookupService userLookupService) : IFamilyMemberInvitationRepository
{
    #region IRepository<TEntity, TId> Methods

    /// <inheritdoc />
    public async Task<FamilyMemberInvitation?> GetByIdAsync(InvitationId id, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(FamilyMemberInvitation invitation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invitation);
        await context.FamilyMemberInvitations.AddAsync(invitation, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(FamilyMemberInvitation entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        context.FamilyMemberInvitations.Update(entity);
    }

    /// <inheritdoc />
    public void Remove(FamilyMemberInvitation entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        context.FamilyMemberInvitations.Remove(entity);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(InvitationId id, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations.AnyAsync(i => i.Id == id, cancellationToken);
    }

    #endregion

    #region ISpecificationRepository<TEntity, TId> Methods

    /// <inheritdoc />
    public async Task<FamilyMemberInvitation?> FindOneAsync(
        IQueryableSpecification<FamilyMemberInvitation> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.FamilyMemberInvitations.AsQueryable(), specification);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Maybe<FamilyMemberInvitation>> FindOneMaybeAsync(
        IQueryableSpecification<FamilyMemberInvitation> specification,
        CancellationToken cancellationToken = default)
    {
        var result = await FindOneAsync(specification, cancellationToken);
        return result is null ? Maybe<FamilyMemberInvitation>.None : Maybe<FamilyMemberInvitation>.Some(result);
    }

    /// <inheritdoc />
    public async Task<List<FamilyMemberInvitation>> FindAllAsync(
        IQueryableSpecification<FamilyMemberInvitation> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.FamilyMemberInvitations.AsQueryable(), specification);
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        IQueryableSpecification<FamilyMemberInvitation> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.FamilyMemberInvitations.AsQueryable(), specification);
        return await query.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AnyAsync(
        IQueryableSpecification<FamilyMemberInvitation> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.FamilyMemberInvitations.AsQueryable(), specification);
        return await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<TResult>> FindAllProjectedAsync<TResult>(
        IProjectionSpecification<FamilyMemberInvitation, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.FamilyMemberInvitations.AsQueryable(), specification);
        return await query.Select(specification.Selector).ToListAsync(cancellationToken);
    }

    #endregion

    #region Module-Specific Methods

    /// <inheritdoc />
    public Task UpdateAsync(FamilyMemberInvitation invitation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invitation);
        // EF Core tracks changes automatically when entities are loaded from the context
        context.FamilyMemberInvitations.Update(invitation);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void RemoveInvitations(List<FamilyMemberInvitation> invitations)
    {
        ArgumentNullException.ThrowIfNull(invitations);
        context.FamilyMemberInvitations.RemoveRange(invitations);
    }

    #endregion

    #region Legacy Methods (Obsolete)

    /// <inheritdoc />
    [Obsolete("Use FindOneAsync with InvitationByTokenSpecification.")]
    public async Task<FamilyMemberInvitation?> GetByTokenAsync(InvitationToken token, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
    }

    /// <inheritdoc />
    [Obsolete("Use FindAllAsync with PendingInvitationByFamilySpecification.")]
    public async Task<List<FamilyMemberInvitation>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .Where(i => i.FamilyId == familyId && i.Status == InvitationStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    [Obsolete("Use FindOneAsync with PendingInvitationByEmailSpecification.")]
    public async Task<FamilyMemberInvitation?> GetPendingByEmailAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .FirstOrDefaultAsync(
                i => i.FamilyId == familyId
                    && i.Email == email
                    && i.Status == InvitationStatus.Pending,
                cancellationToken);
    }

    /// <inheritdoc />
    [Obsolete("Use FindAllAsync with InvitationsByFamilySpecification (create if needed).")]
    public async Task<List<FamilyMemberInvitation>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .Where(i => i.FamilyId == familyId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    [Obsolete("Use cross-module service with UsersByFamilySpecification.")]
    public async Task<bool> IsUserMemberOfFamilyAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default)
    {
        // Use IUserLookupService for cross-module query to Auth module
        return await userLookupService.IsEmailMemberOfFamilyAsync(familyId, email, cancellationToken);
    }

    /// <inheritdoc />
    [Obsolete("Use FindAllAsync with ExpiredInvitationForCleanupSpecification.")]
    public async Task<List<FamilyMemberInvitation>> GetExpiredInvitationsForCleanupAsync(DateTime expirationThreshold, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .Where(i => i.ExpiresAt < expirationThreshold && i.Status == InvitationStatus.Expired)
            .ToListAsync(cancellationToken);
    }

    #endregion
}
