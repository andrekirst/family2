using FamilyHub.Modules.UserProfile.Domain.Aggregates;
using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.UserProfile.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the ProfileChangeRequest repository using UserProfileDbContext.
/// Implements specification-based queries for flexible and testable data access.
/// </summary>
/// <param name="context">The UserProfile module database context.</param>
public sealed class ProfileChangeRequestRepository(UserProfileDbContext context) : IProfileChangeRequestRepository
{
    #region IRepository<TEntity, TId> Methods

    /// <inheritdoc />
    public async Task<ProfileChangeRequest?> GetByIdAsync(ChangeRequestId id, CancellationToken cancellationToken = default)
    {
        return await context.ProfileChangeRequests
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(ProfileChangeRequest entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await context.ProfileChangeRequests.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(ProfileChangeRequest entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        context.ProfileChangeRequests.Update(entity);
    }

    /// <inheritdoc />
    public void Remove(ProfileChangeRequest entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        context.ProfileChangeRequests.Remove(entity);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(ChangeRequestId id, CancellationToken cancellationToken = default)
    {
        return await context.ProfileChangeRequests.AnyAsync(r => r.Id == id, cancellationToken);
    }

    #endregion

    #region ISpecificationRepository<TEntity, TId> Methods

    /// <inheritdoc />
    public async Task<ProfileChangeRequest?> FindOneAsync(
        IQueryableSpecification<ProfileChangeRequest> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.ProfileChangeRequests.AsQueryable(), specification);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Maybe<ProfileChangeRequest>> FindOneMaybeAsync(
        IQueryableSpecification<ProfileChangeRequest> specification,
        CancellationToken cancellationToken = default)
    {
        var result = await FindOneAsync(specification, cancellationToken);
        return result is null ? Maybe<ProfileChangeRequest>.None : Maybe<ProfileChangeRequest>.Some(result);
    }

    /// <inheritdoc />
    public async Task<List<ProfileChangeRequest>> FindAllAsync(
        IQueryableSpecification<ProfileChangeRequest> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.ProfileChangeRequests.AsQueryable(), specification);
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        IQueryableSpecification<ProfileChangeRequest> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.ProfileChangeRequests.AsQueryable(), specification);
        return await query.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AnyAsync(
        IQueryableSpecification<ProfileChangeRequest> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.ProfileChangeRequests.AsQueryable(), specification);
        return await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<TResult>> FindAllProjectedAsync<TResult>(
        IProjectionSpecification<ProfileChangeRequest, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.ProfileChangeRequests.AsQueryable(), specification);
        return await query.Select(specification.Selector).ToListAsync(cancellationToken);
    }

    #endregion

    #region Custom Methods

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProfileChangeRequest>> GetPendingByFamilyAsync(
        FamilyId familyId,
        CancellationToken cancellationToken = default)
    {
        return await context.ProfileChangeRequests
            .Where(r => r.FamilyId == familyId && r.Status == ChangeRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProfileChangeRequest>> GetPendingByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        return await context.ProfileChangeRequests
            .Where(r => r.RequestedBy == userId && r.Status == ChangeRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProfileChangeRequest?> GetPendingByProfileAndFieldAsync(
        UserProfileId profileId,
        string fieldName,
        CancellationToken cancellationToken = default)
    {
        return await context.ProfileChangeRequests
            .FirstOrDefaultAsync(r =>
                r.ProfileId == profileId &&
                r.FieldName == fieldName &&
                r.Status == ChangeRequestStatus.Pending,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProfileChangeRequest>> GetAllByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        return await context.ProfileChangeRequests
            .Where(r => r.RequestedBy == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    #endregion
}
