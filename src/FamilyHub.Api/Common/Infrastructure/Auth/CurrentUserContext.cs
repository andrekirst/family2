using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Common.Infrastructure.Auth;

/// <summary>
/// Scoped implementation of <see cref="ICurrentUserContext"/> that lazily resolves
/// the current user from the database on first access. Uses <see cref="IHttpContextAccessor"/>
/// to extract the JWT "sub" claim and looks up the corresponding user record.
/// </summary>
public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;
    private readonly Lazy<Task<CurrentUserInfo>> _lazyUser;

    public CurrentUserContext(
        IHttpContextAccessor httpContextAccessor,
        IUserRepository userRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
        _lazyUser = new Lazy<Task<CurrentUserInfo>>(ResolveUserAsync);
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public Task<CurrentUserInfo> GetCurrentUserAsync() => _lazyUser.Value;

    public RawClaimsInfo GetRawClaims()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new UserNotAuthenticatedException();

        var externalUserIdString = httpContext.User.FindFirst(ClaimNames.Standard.Sub)?.Value;
        if (string.IsNullOrEmpty(externalUserIdString))
        {
            throw new UserNotAuthenticatedException();
        }

        var email = httpContext.User.FindFirst(ClaimNames.Standard.Email)?.Value
            ?? throw new UserNotAuthenticatedException();

        var emailVerifiedString = httpContext.User.FindFirst(ClaimNames.Standard.EmailVerified)?.Value;
        var emailVerified = bool.TryParse(emailVerifiedString, out var ev) && ev;

        var userName = httpContext.User.FindFirst(ClaimNames.Standard.Name)?.Value;

        return new RawClaimsInfo(
            ExternalUserId.From(externalUserIdString),
            email,
            emailVerified,
            userName);
    }

    private async Task<CurrentUserInfo> ResolveUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new UserNotAuthenticatedException();

        var externalUserIdString = httpContext.User.FindFirst(ClaimNames.Standard.Sub)?.Value;
        if (string.IsNullOrEmpty(externalUserIdString))
        {
            throw new UserNotAuthenticatedException();
        }

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var user = await _userRepository.GetByExternalIdAsync(externalUserId, CancellationToken.None)
            ?? throw new UserNotFoundException();

        return new CurrentUserInfo(
            user.Id,
            externalUserId,
            user.FamilyId);
    }
}
