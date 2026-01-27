using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace FamilyHub.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Reads current user identity from X-Test-User-Id, X-Test-User-Email, and X-Test-Family-Id headers.
/// Used for E2E testing without real JWT tokens.
/// </summary>
/// <remarks>
/// <para>
/// SECURITY: This service should ONLY be registered when TestMode is enabled.
/// It bypasses all JWT validation and trusts the headers completely.
/// </para>
/// <para>
/// This service allows Playwright E2E tests and k6 performance tests to simulate
/// authenticated users by passing user identity in HTTP headers instead of
/// requiring valid JWT tokens.
/// </para>
/// <para>
/// Example E2E usage:
/// <code>
/// const response = await apiContext.post('/graphql', {
///     headers: {
///         'Content-Type': 'application/json',
///         'X-Test-User-Id': '00000000-0000-0000-0000-000000000001',
///         'X-Test-User-Email': 'test@familyhub.test',
///         'X-Test-Family-Id': '00000000-0000-0000-0000-000000000002'
///     },
///     data: { query: '{ family { id } }' }
/// });
/// </code>
/// </para>
/// </remarks>
/// <param name="httpContextAccessor">Accessor for the HTTP context containing request headers.</param>
public sealed class HeaderBasedCurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    /// <summary>
    /// Header name for test user ID.
    /// </summary>
    public const string TestUserIdHeader = "X-Test-User-Id";

    /// <summary>
    /// Header name for test user email.
    /// </summary>
    public const string TestUserEmailHeader = "X-Test-User-Email";

    /// <summary>
    /// Header name for test family ID.
    /// </summary>
    public const string TestFamilyIdHeader = "X-Test-Family-Id";

    /// <inheritdoc />
    public bool IsAuthenticated => !string.IsNullOrEmpty(GetUserIdHeader());

    /// <inheritdoc />
    public UserId GetUserId()
    {
        var userId = TryGetUserId();
        if (userId is null)
        {
            throw new UnauthorizedAccessException(
                $"{TestUserIdHeader} header is required in Test mode. " +
                "Provide a valid GUID representing the test user's ID.");
        }

        return userId.Value;
    }

    /// <inheritdoc />
    public Task<UserId> GetUserIdAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(GetUserId());

    /// <inheritdoc />
    public Email? GetUserEmail()
    {
        var header = httpContextAccessor.HttpContext?.Request.Headers[TestUserEmailHeader].FirstOrDefault();

        return string.IsNullOrEmpty(header) ? null : Email.From(header);
    }

    /// <inheritdoc />
    public FamilyId? GetFamilyId() => TryGetFamilyId();

    /// <inheritdoc />
    public UserId? TryGetUserId()
    {
        var header = GetUserIdHeader();

        if (string.IsNullOrEmpty(header))
        {
            return null;
        }

        if (!Guid.TryParse(header, out var guid))
        {
            return null;
        }

        return UserId.From(guid);
    }

    /// <inheritdoc />
    public FamilyId? TryGetFamilyId()
    {
        var header = httpContextAccessor.HttpContext?.Request.Headers[TestFamilyIdHeader].FirstOrDefault();

        if (string.IsNullOrEmpty(header))
        {
            return null;
        }

        if (!Guid.TryParse(header, out var guid))
        {
            return null;
        }

        return FamilyId.From(guid);
    }

    private string? GetUserIdHeader()
        => httpContextAccessor.HttpContext?.Request.Headers[TestUserIdHeader].FirstOrDefault();
}
