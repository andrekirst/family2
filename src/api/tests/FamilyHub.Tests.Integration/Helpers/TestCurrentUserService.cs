using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Test implementation of ICurrentUserService that allows setting the user ID for integration tests.
/// Thread-safe using AsyncLocal to support concurrent test execution.
/// </summary>
public sealed class TestCurrentUserService : ICurrentUserService
{
    private static readonly AsyncLocal<UserId?> UserId = new();
    private static readonly AsyncLocal<Email?> UserEmail = new();

    /// <summary>
    /// Sets the current user ID for this async context.
    /// </summary>
    public static void SetUserId(UserId userId) => UserId.Value = userId;

    /// <summary>
    /// Sets the current user email for this async context.
    /// </summary>
    public static void SetUserEmail(Email email) => UserEmail.Value = email;

    /// <summary>
    /// Clears the current user ID for this async context.
    /// </summary>
    public static void ClearUserId()
    {
        UserId.Value = null;
        UserEmail.Value = null;
    }

    /// <summary>
    /// Gets the current user ID from the async context.
    /// Throws UnauthorizedAccessException if no user ID is set (matching production behavior).
    /// </summary>
    public UserId GetUserId() =>
        UserId.Value ?? throw new UnauthorizedAccessException("User is not authenticated in test context.");

    /// <summary>
    /// Gets the current user ID from the async context asynchronously.
    /// Throws UnauthorizedAccessException if no user ID is set (matching production behavior).
    /// </summary>
    public Task<UserId> GetUserIdAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(UserId.Value ?? throw new UnauthorizedAccessException("User is not authenticated in test context."));

    /// <summary>
    /// Gets the current user email from the async context.
    /// </summary>
    public Email? GetUserEmail() => UserEmail.Value;

    /// <summary>
    /// Checks if the current user is authenticated (has a user ID set).
    /// </summary>
    public bool IsAuthenticated => UserId.Value != null;
}
