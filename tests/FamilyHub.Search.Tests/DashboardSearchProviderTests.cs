using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Dashboard.Application.Search;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.Search.Tests;

public class DashboardSearchProviderTests
{
    private static readonly UserId TestUserId = UserId.From(Guid.NewGuid());
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());

    [Fact]
    public async Task SearchAsync_MatchesPersonalDashboard()
    {
        var repo = new FakeDashboardLayoutRepository();
        var personal = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("My Dashboard"), TestUserId);
        personal.ClearDomainEvents();
        repo.Seed(personal);

        var provider = new DashboardSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "dashboard");

        var results = await provider.SearchAsync(context);

        results.Should().HaveCount(1);
        results[0].Title.Should().Be("My Dashboard");
        results[0].Description.Should().Be("Personal dashboard");
    }

    [Fact]
    public async Task SearchAsync_MatchesSharedDashboard()
    {
        var repo = new FakeDashboardLayoutRepository();
        var shared = DashboardLayout.CreateShared(
            DashboardLayoutName.From("Family Board"), TestFamilyId, TestUserId);
        shared.ClearDomainEvents();
        repo.Seed(shared);

        var provider = new DashboardSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "family");

        var results = await provider.SearchAsync(context);

        results.Should().HaveCount(1);
        results[0].Title.Should().Be("Family Board");
        results[0].Description.Should().Be("Shared family dashboard");
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsEmpty()
    {
        var repo = new FakeDashboardLayoutRepository();
        var personal = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("My Dashboard"), TestUserId);
        personal.ClearDomainEvents();
        repo.Seed(personal);

        var provider = new DashboardSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "");

        var results = await provider.SearchAsync(context);

        results.Should().BeEmpty();
    }

    [Fact]
    public void ModuleName_ShouldBeDashboard()
    {
        var repo = new FakeDashboardLayoutRepository();
        var provider = new DashboardSearchProvider(repo);

        provider.ModuleName.Should().Be("dashboard");
    }
}
