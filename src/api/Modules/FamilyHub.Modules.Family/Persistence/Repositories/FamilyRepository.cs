using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Modules.Family.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the Family repository using FamilyDbContext.
///
/// PHASE 5 STATE: Repository now resides in Family module with its own DbContext.
///
/// CROSS-MODULE QUERIES:
/// - GetFamilyByUserIdAsync and GetMemberCountAsync require data from Auth module
/// - These queries are handled via IUserLookupService abstraction
/// - This maintains bounded context separation while enabling necessary cross-module operations
///
/// SPECIFICATION PATTERN:
/// - Implements ISpecificationRepository for specification-based queries
/// - Use FindOneAsync/FindAllAsync with specifications for new code
/// - Legacy methods are marked [Obsolete] for gradual migration
/// </summary>
/// <param name="context">The Family module database context.</param>
/// <param name="userLookupService">Service for cross-module user lookups.</param>
public sealed class FamilyRepository(FamilyDbContext context, IUserLookupService userLookupService) : IFamilyRepository
{
    #region IRepository<TEntity, TId> Methods

    /// <inheritdoc />
    public async Task<FamilyAggregate?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken = default)
    {
        return await context.Families
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(FamilyAggregate family, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(family);
        await context.Families.AddAsync(family, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(FamilyAggregate entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        context.Families.Update(entity);
    }

    /// <inheritdoc />
    public void Remove(FamilyAggregate entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        context.Families.Remove(entity);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(FamilyId id, CancellationToken cancellationToken = default)
    {
        return await context.Families.AnyAsync(f => f.Id == id, cancellationToken);
    }

    #endregion

    #region ISpecificationRepository<TEntity, TId> Methods

    /// <inheritdoc />
    public async Task<FamilyAggregate?> FindOneAsync(
        IQueryableSpecification<FamilyAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Families.AsQueryable(), specification);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Maybe<FamilyAggregate>> FindOneMaybeAsync(
        IQueryableSpecification<FamilyAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        var result = await FindOneAsync(specification, cancellationToken);
        return result is null ? Maybe<FamilyAggregate>.None : Maybe<FamilyAggregate>.Some(result);
    }

    /// <inheritdoc />
    public async Task<List<FamilyAggregate>> FindAllAsync(
        IQueryableSpecification<FamilyAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Families.AsQueryable(), specification);
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        IQueryableSpecification<FamilyAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Families.AsQueryable(), specification);
        return await query.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AnyAsync(
        IQueryableSpecification<FamilyAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Families.AsQueryable(), specification);
        return await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<TResult>> FindAllProjectedAsync<TResult>(
        IProjectionSpecification<FamilyAggregate, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Families.AsQueryable(), specification);
        return await query.Select(specification.Selector).ToListAsync(cancellationToken);
    }

    #endregion

    #region Legacy Methods (Obsolete)

    /// <inheritdoc />
    [Obsolete("Use FindOneAsync with FamilyByOwnerSpecification or cross-module lookup.")]
    public async Task<FamilyAggregate?> GetFamilyByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        // Use IUserLookupService for cross-module query
        var familyId = await userLookupService.GetUserFamilyIdAsync(userId, cancellationToken);

        if (familyId == null)
        {
            return null;
        }

        return await GetByIdAsync(familyId.Value, cancellationToken);
    }

    /// <inheritdoc />
    [Obsolete("Use CountAsync with UsersByFamilySpecification via cross-module service.")]
    public async Task<int> GetMemberCountAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        // Use IUserLookupService for cross-module query
        return await userLookupService.GetFamilyMemberCountAsync(familyId, cancellationToken);
    }

    #endregion
}
