using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the User repository.
///
/// SPECIFICATION PATTERN:
/// - Implements ISpecificationRepository for specification-based queries
/// - Use FindOneAsync/FindAllAsync with specifications for new code
/// - Legacy methods are marked [Obsolete] for gradual migration
/// </summary>
/// <param name="context">The Auth module database context.</param>
public sealed class UserRepository(AuthDbContext context) : IUserRepository
{
    #region IRepository<TEntity, TId> Methods

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        await context.Users.AddAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // In EF Core with change tracking, this is often not necessary
        // EF tracks changes automatically when entities are loaded from the context
        // However, we include it for explicit updates from detached entities
        context.Users.Update(user);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Remove(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        context.Users.Remove(user);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    #endregion

    #region ISpecificationRepository<TEntity, TId> Methods

    /// <inheritdoc />
    public async Task<User?> FindOneAsync(
        IQueryableSpecification<User> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Users.AsQueryable(), specification);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Maybe<User>> FindOneMaybeAsync(
        IQueryableSpecification<User> specification,
        CancellationToken cancellationToken = default)
    {
        var result = await FindOneAsync(specification, cancellationToken);
        return result is null ? Maybe<User>.None : Maybe<User>.Some(result);
    }

    /// <inheritdoc />
    public async Task<List<User>> FindAllAsync(
        IQueryableSpecification<User> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Users.AsQueryable(), specification);
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        IQueryableSpecification<User> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Users.AsQueryable(), specification);
        return await query.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AnyAsync(
        IQueryableSpecification<User> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Users.AsQueryable(), specification);
        return await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<TResult>> FindAllProjectedAsync<TResult>(
        IProjectionSpecification<User, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.GetQuery(context.Users.AsQueryable(), specification);
        return await query.Select(specification.Selector).ToListAsync(cancellationToken);
    }

    #endregion

    #region Password Reset and Email Verification Methods

    /// <inheritdoc />
    public async Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token, cancellationToken);
    }

    #endregion

    #region Legacy Methods (Obsolete)

    /// <inheritdoc />
    [Obsolete("Use FindOneAsync with UserByEmailSpecification.")]
    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    [Obsolete("Use AnyAsync with UserByEmailSpecification.")]
    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    [Obsolete("Use FindAllAsync with UsersByFamilySpecification.")]
    public async Task<List<User>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Where(u => u.FamilyId == familyId)
            .ToListAsync(cancellationToken);
    }

    #endregion
}
