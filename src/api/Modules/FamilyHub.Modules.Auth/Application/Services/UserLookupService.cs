using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Application.Services;

/// <summary>
/// Implementation of IUserLookupService providing cross-module user queries.
/// Used by Family module to query user data without direct DbContext access.
///
/// PURPOSE: Enables Family module repositories to perform necessary user lookups
/// while maintaining proper bounded context separation.
///
/// ARCHITECTURE:
/// - Implements interface from SharedKernel
/// - Uses AuthDbContext (owns User data)
/// - Returns only value objects/primitives (no entity leakage)
/// - All queries use AsNoTracking() for read-only performance
/// </summary>
/// <param name="context">The Auth module database context.</param>
public sealed class UserLookupService(AuthDbContext context) : IUserLookupService
{
    /// <inheritdoc />
    public async Task<FamilyId?> GetUserFamilyIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user?.FamilyId;
    }

    /// <inheritdoc />
    public async Task<int> GetFamilyMemberCountAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Where(u => u.FamilyId == familyId)
            .CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> IsEmailMemberOfFamilyAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AnyAsync(
                u => u.FamilyId == familyId && u.Email == email,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FamilyId?> GetFamilyIdByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return user?.FamilyId;
    }
}
