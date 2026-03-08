using System.Globalization;
using System.Security.Claims;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Middleware;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyHub.Auth.Tests.Common.Middleware;

public class RequestLocaleResolutionMiddlewareTests
{
    private static readonly ExternalUserId TestExternalUserId =
        ExternalUserId.From(Guid.NewGuid().ToString());

    private static User CreateUser(string preferredLocale = "en")
    {
        var user = User.Register(
            Email.From("test@example.com"),
            UserName.From("Test User"),
            TestExternalUserId,
            emailVerified: true);
        user.UpdateLocale(preferredLocale);
        return user;
    }

    private static DefaultHttpContext CreateAuthenticatedContext(string subClaimValue)
    {
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(
            [new Claim(ClaimNames.Standard.Sub, subClaimValue)],
            authenticationType: "Bearer");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }

    private static DefaultHttpContext CreateUnauthenticatedContext()
    {
        return new DefaultHttpContext();
    }

    private static IMemoryCache CreateMemoryCache() => new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public async Task Should_default_to_en_when_unauthenticated_and_no_accept_language()
    {
        var userRepo = new FakeUserRepository();
        CultureInfo? capturedCulture = null;
        var middleware = new RequestLocaleResolutionMiddleware(_ =>
        {
            capturedCulture = CultureInfo.CurrentUICulture;
            return Task.CompletedTask;
        });
        var context = CreateUnauthenticatedContext();

        await middleware.InvokeAsync(context, userRepo, CreateMemoryCache());

        capturedCulture!.Name.Should().Be("en");
    }

    [Fact]
    public async Task Should_use_accept_language_when_unauthenticated()
    {
        var userRepo = new FakeUserRepository();
        CultureInfo? capturedCulture = null;
        var middleware = new RequestLocaleResolutionMiddleware(_ =>
        {
            capturedCulture = CultureInfo.CurrentUICulture;
            return Task.CompletedTask;
        });
        var context = CreateUnauthenticatedContext();
        context.Request.Headers.AcceptLanguage = "de,en;q=0.9";

        await middleware.InvokeAsync(context, userRepo, CreateMemoryCache());

        capturedCulture!.Name.Should().Be("de");
    }

    [Fact]
    public async Task Should_use_user_preferred_locale_when_authenticated()
    {
        var user = CreateUser("de");
        var userRepo = new FakeUserRepository(user);
        CultureInfo? capturedCulture = null;
        var middleware = new RequestLocaleResolutionMiddleware(_ =>
        {
            capturedCulture = CultureInfo.CurrentUICulture;
            return Task.CompletedTask;
        });
        var context = CreateAuthenticatedContext(TestExternalUserId.Value);

        await middleware.InvokeAsync(context, userRepo, CreateMemoryCache());

        capturedCulture!.Name.Should().Be("de");
    }

    [Fact]
    public async Task Should_fallback_to_accept_language_when_user_has_no_preferred_locale()
    {
        // User with default "en" locale but we set Accept-Language to "de"
        // Note: FakeUserRepository returns null when no user is provided
        var userRepo = new FakeUserRepository();
        CultureInfo? capturedCulture = null;
        var middleware = new RequestLocaleResolutionMiddleware(_ =>
        {
            capturedCulture = CultureInfo.CurrentUICulture;
            return Task.CompletedTask;
        });
        var context = CreateAuthenticatedContext(TestExternalUserId.Value);
        context.Request.Headers.AcceptLanguage = "de";

        await middleware.InvokeAsync(context, userRepo, CreateMemoryCache());

        capturedCulture!.Name.Should().Be("de");
    }

    [Fact]
    public async Task Should_default_to_en_when_authenticated_but_user_not_found_and_no_accept_language()
    {
        var userRepo = new FakeUserRepository();
        CultureInfo? capturedCulture = null;
        var middleware = new RequestLocaleResolutionMiddleware(_ =>
        {
            capturedCulture = CultureInfo.CurrentUICulture;
            return Task.CompletedTask;
        });
        var context = CreateAuthenticatedContext(TestExternalUserId.Value);

        await middleware.InvokeAsync(context, userRepo, CreateMemoryCache());

        capturedCulture!.Name.Should().Be("en");
    }

    [Fact]
    public async Task Should_set_both_current_culture_and_current_ui_culture()
    {
        var user = CreateUser("de");
        var userRepo = new FakeUserRepository(user);
        CultureInfo? capturedCulture = null;
        CultureInfo? capturedUiCulture = null;
        var middleware = new RequestLocaleResolutionMiddleware(_ =>
        {
            capturedCulture = CultureInfo.CurrentCulture;
            capturedUiCulture = CultureInfo.CurrentUICulture;
            return Task.CompletedTask;
        });
        var context = CreateAuthenticatedContext(TestExternalUserId.Value);

        await middleware.InvokeAsync(context, userRepo, CreateMemoryCache());

        capturedCulture!.Name.Should().Be("de");
        capturedUiCulture!.Name.Should().Be("de");
    }

    [Fact]
    public async Task Should_call_next_middleware()
    {
        var userRepo = new FakeUserRepository();
        var nextCalled = false;
        var middleware = new RequestLocaleResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateUnauthenticatedContext();

        await middleware.InvokeAsync(context, userRepo, CreateMemoryCache());

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_default_to_en_when_sub_claim_is_not_a_valid_guid()
    {
        var userRepo = new FakeUserRepository();
        CultureInfo? capturedCulture = null;
        var middleware = new RequestLocaleResolutionMiddleware(_ =>
        {
            capturedCulture = CultureInfo.CurrentUICulture;
            return Task.CompletedTask;
        });
        var context = CreateAuthenticatedContext("not-a-guid");

        await middleware.InvokeAsync(context, userRepo, CreateMemoryCache());

        capturedCulture!.Name.Should().Be("en");
    }

    [Fact]
    public async Task Should_parse_first_language_from_complex_accept_language_header()
    {
        var userRepo = new FakeUserRepository();
        CultureInfo? capturedCulture = null;
        var middleware = new RequestLocaleResolutionMiddleware(_ =>
        {
            capturedCulture = CultureInfo.CurrentUICulture;
            return Task.CompletedTask;
        });
        var context = CreateUnauthenticatedContext();
        context.Request.Headers.AcceptLanguage = "fr-FR, de;q=0.8, en;q=0.5";

        await middleware.InvokeAsync(context, userRepo, CreateMemoryCache());

        capturedCulture!.Name.Should().Be("fr-FR");
    }

    [Fact]
    public async Task Should_use_cached_locale_on_second_request()
    {
        var user = CreateUser("de");
        var userRepo = new FakeUserRepository(user);
        var cache = CreateMemoryCache();
        CultureInfo? capturedCulture = null;
        var middleware = new RequestLocaleResolutionMiddleware(_ =>
        {
            capturedCulture = CultureInfo.CurrentUICulture;
            return Task.CompletedTask;
        });

        // First request — populates cache
        var context1 = CreateAuthenticatedContext(TestExternalUserId.Value);
        await middleware.InvokeAsync(context1, userRepo, cache);
        capturedCulture!.Name.Should().Be("de");

        // Second request with same cache — should still return "de" even if repo is empty
        var emptyRepo = new FakeUserRepository();
        var context2 = CreateAuthenticatedContext(TestExternalUserId.Value);
        await middleware.InvokeAsync(context2, emptyRepo, cache);
        capturedCulture!.Name.Should().Be("de");
    }
}
