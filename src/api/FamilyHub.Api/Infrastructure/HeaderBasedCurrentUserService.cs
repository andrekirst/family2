using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Api.Infrastructure;

/// <summary>
/// Reads current user identity from X-Test-User-Id header.
/// Used for k6 performance testing without real JWT tokens.
/// </summary>
/// <remarks>
/// This service is ONLY registered when ASPNETCORE_ENVIRONMENT=Test.
/// It allows k6 tests to simulate authenticated users by passing a user ID
/// in the X-Test-User-Id header instead of requiring a valid JWT token.
///
/// Example k6 usage:
/// <code>
/// const response = http.post(graphqlUrl, JSON.stringify({ query }), {
///     headers: {
///         'Content-Type': 'application/json',
///         'X-Test-User-Id': '00000000-0000-0000-0000-000000000001'
///     }
/// });
/// </code>
/// </remarks>
/// <param name="httpContextAccessor">Accessor for the HTTP context containing request headers.</param>
public sealed class HeaderBasedCurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    private const string TestUserIdHeader = "X-Test-User-Id";
    private const string TestUserEmailHeader = "X-Test-User-Email";

    /// <inheritdoc />
    public bool IsAuthenticated => !string.IsNullOrEmpty(GetUserIdHeader());

    /// <inheritdoc />
    public UserId GetUserId()
    {
        var header = GetUserIdHeader();

        if (string.IsNullOrEmpty(header))
        {
            throw new UnauthorizedAccessException(
                $"{TestUserIdHeader} header is required in Test environment. " +
                "Provide a valid GUID representing the test user's ID.");
        }

        return !Guid.TryParse(header, out var guid)
            ? throw new UnauthorizedAccessException($"{TestUserIdHeader} header must be a valid GUID. Received: '{header}'")
            : UserId.From(guid);
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

    private string? GetUserIdHeader()
        => httpContextAccessor.HttpContext?.Request.Headers[TestUserIdHeader].FirstOrDefault();
}
