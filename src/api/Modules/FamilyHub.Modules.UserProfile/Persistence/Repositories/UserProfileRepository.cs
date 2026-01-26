using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Domain.Specifications;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using UserProfileAggregate = FamilyHub.Modules.UserProfile.Domain.Aggregates.UserProfile;

namespace FamilyHub.Modules.UserProfile.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the UserProfile repository using UserProfileDbContext.
/// Implements specification-based queries for flexible and testable data access.
/// </summary>
/// <param name="context">The UserProfile module database context.</param>
public sealed class UserProfileRepository(UserProfileDbContext context) : IUserProfileRepository
{
    #region IRepository<TEntity, TId> Methods

    /// <inheritdoc />
    public async Task<UserProfileAggregate?> GetByIdAsync(UserProfileId id, CancellationToken cancellationToken = default)
    {
        return await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(UserProfileAggregate profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        await context.Profiles.AddAsync(profile, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(UserProfileAggregate entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        context.Profiles.Update(entity);
    }

    /// <inheritdoc />
    public void Remove(UserProfileAggregate entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        context.Profiles.Remove(entity);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(UserProfileId id, CancellationToken cancellationToken = default)
    {
        return await context.Profiles.AnyAsync(p => p.Id == id, cancellationToken);
    }

    #endregion

    #region ISpecificationRepository<TEntity, TId> Methods

    /// <inheritdoc />
    public async Task<UserProfileAggregate?> FindOneAsync(
        IQueryableSpecification<UserProfileAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Profiles.AsQueryable(), specification);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Maybe<UserProfileAggregate>> FindOneMaybeAsync(
        IQueryableSpecification<UserProfileAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        var result = await FindOneAsync(specification, cancellationToken);
        return result is null ? Maybe<UserProfileAggregate>.None : Maybe<UserProfileAggregate>.Some(result);
    }

    /// <inheritdoc />
    public async Task<List<UserProfileAggregate>> FindAllAsync(
        IQueryableSpecification<UserProfileAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Profiles.AsQueryable(), specification);
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        IQueryableSpecification<UserProfileAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Profiles.AsQueryable(), specification);
        return await query.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AnyAsync(
        IQueryableSpecification<UserProfileAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Profiles.AsQueryable(), specification);
        return await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<TResult>> FindAllProjectedAsync<TResult>(
        IProjectionSpecification<UserProfileAggregate, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Profiles.AsQueryable(), specification);
        return await query.Select(specification.Selector).ToListAsync(cancellationToken);
    }

    #endregion

    #region Custom Methods

    /// <inheritdoc />
    public async Task<UserProfileAggregate?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await FindOneAsync(new ProfileByUserIdSpecification(userId), cancellationToken);
    }

    #endregion
}
