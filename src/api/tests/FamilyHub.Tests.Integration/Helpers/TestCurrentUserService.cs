using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Test implementation of ICurrentUserService that allows setting the user ID for integration tests.
/// Thread-safe using AsyncLocal to support concurrent test execution.
/// </summary>
public sealed class TestCurrentUserService : ICurrentUserService
{
    private static readonly AsyncLocal<UserId?> _userId = new();
    private static readonly AsyncLocal<Email?> _userEmail = new();

    /// <summary>
    /// Sets the current user ID for this async context.
    /// </summary>
    public static void SetUserId(UserId userId) => _userId.Value = userId;

    /// <summary>
    /// Sets the current user email for this async context.
    /// </summary>
    public static void SetUserEmail(Email email) => _userEmail.Value = email;

    /// <summary>
    /// Clears the current user ID for this async context.
    /// </summary>
    public static void ClearUserId()
    {
        _userId.Value = null;
        _userEmail.Value = null;
    }

    /// <summary>
    /// Gets the current user ID from the async context.
    /// Throws UnauthorizedAccessException if no user ID is set (matching production behavior).
    /// </summary>
    public UserId GetUserId() =>
        _userId.Value ?? throw new UnauthorizedAccessException("User is not authenticated in test context.");

    /// <summary>
    /// Gets the current user ID from the async context asynchronously.
    /// Throws UnauthorizedAccessException if no user ID is set (matching production behavior).
    /// </summary>
    public Task<UserId> GetUserIdAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_userId.Value ?? throw new UnauthorizedAccessException("User is not authenticated in test context."));

    /// <summary>
    /// Gets the current user email from the async context.
    /// </summary>
    public Email? GetUserEmail() => _userEmail.Value;

    /// <summary>
    /// Checks if the current user is authenticated (has a user ID set).
    /// </summary>
    public bool IsAuthenticated => _userId.Value != null;
}
