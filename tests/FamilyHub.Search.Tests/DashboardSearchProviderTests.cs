using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Dashboard.Application.Search;
using FamilyHub.Api.Features.Dashboard.Domain.Entities;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Search.Tests;

public class DashboardSearchProviderTests
{
    private static readonly UserId TestUserId = UserId.From(Guid.NewGuid());
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());

    [Fact]
    public async Task SearchAsync_MatchesPersonalDashboard()
    {
        var repo = Substitute.For<IDashboardLayoutRepository>();
        var personal = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("My Dashboard"), TestUserId);
        personal.ClearDomainEvents();

        repo.GetPersonalDashboardAsync(TestUserId, Arg.Any<CancellationToken>())
            .Returns(personal);

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
        var repo = Substitute.For<IDashboardLayoutRepository>();
        var shared = DashboardLayout.CreateShared(
            DashboardLayoutName.From("Family Board"), TestFamilyId, TestUserId);
        shared.ClearDomainEvents();

        repo.GetSharedDashboardAsync(TestFamilyId, Arg.Any<CancellationToken>())
            .Returns(shared);

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
        var repo = Substitute.For<IDashboardLayoutRepository>();
        var personal = DashboardLayout.CreatePersonal(
            DashboardLayoutName.From("My Dashboard"), TestUserId);
        personal.ClearDomainEvents();

        repo.GetPersonalDashboardAsync(TestUserId, Arg.Any<CancellationToken>())
            .Returns(personal);

        var provider = new DashboardSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "");

        var results = await provider.SearchAsync(context);

        results.Should().BeEmpty();
    }

    [Fact]
    public void ModuleName_ShouldBeDashboard()
    {
        var repo = Substitute.For<IDashboardLayoutRepository>();
        var provider = new DashboardSearchProvider(repo);

        provider.ModuleName.Should().Be("dashboard");
    }
}
