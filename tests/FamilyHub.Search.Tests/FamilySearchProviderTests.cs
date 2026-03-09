using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Family.Application.Search;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.Search.Tests;

public class FamilySearchProviderTests
{
    private static readonly UserId TestUserId = UserId.From(Guid.NewGuid());
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());

    [Fact]
    public async Task SearchAsync_NoFamily_ShouldReturnEmpty()
    {
        var provider = CreateProvider();
        var context = new SearchContext(TestUserId, null, "test");

        var results = await provider.SearchAsync(context);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_NoMembers_ShouldReturnEmpty()
    {
        var provider = CreateProvider();
        var context = new SearchContext(TestUserId, TestFamilyId, "test");

        var results = await provider.SearchAsync(context);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ShouldRespectLimit()
    {
        var provider = CreateProvider();
        var context = new SearchContext(TestUserId, TestFamilyId, "test", Limit: 1);

        var results = await provider.SearchAsync(context);

        results.Count.Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    public void ModuleName_ShouldBeFamily()
    {
        var provider = CreateProvider();

        provider.ModuleName.Should().Be("family");
    }

    [Fact]
    public async Task SearchAsync_MatchesFamilyName()
    {
        var family = FamilyEntity.Create(FamilyName.From("Smith Family"), TestUserId);
        family.ClearDomainEvents();

        var familyRepo = Substitute.For<IFamilyRepository>();
        familyRepo.GetByIdAsync(family.Id, Arg.Any<CancellationToken>())
            .Returns(family);

        var provider = CreateProvider(familyRepo: familyRepo, familyId: family.Id);
        var context = new SearchContext(TestUserId, family.Id, "smith");

        var results = await provider.SearchAsync(context);

        results.Should().Contain(r => r.Title == "Smith Family" && r.Icon == "home");
    }

    [Fact]
    public async Task SearchAsync_MatchesFamilyName_GermanLocale()
    {
        var family = FamilyEntity.Create(FamilyName.From("Muller Familie"), TestUserId);
        family.ClearDomainEvents();

        var familyRepo = Substitute.For<IFamilyRepository>();
        familyRepo.GetByIdAsync(family.Id, Arg.Any<CancellationToken>())
            .Returns(family);

        var provider = CreateProvider(familyRepo: familyRepo, familyId: family.Id);
        var context = new SearchContext(TestUserId, family.Id, "muller", Locale: "de");

        var results = await provider.SearchAsync(context);

        results.Should().Contain(r => r.Title == "Muller Familie" && r.Description == "Deine Familie");
    }

    [Fact]
    public async Task SearchAsync_MatchesPendingInvitations()
    {
        var invitationRepo = Substitute.For<IFamilyInvitationRepository>();
        var invitation = FamilyInvitation.Create(
            TestFamilyId,
            TestUserId,
            Email.From("alice@example.com"),
            FamilyRole.Member,
            InvitationToken.From("a".PadRight(64, 'a')),
            "plaintext-token");
        invitation.ClearDomainEvents();

        invitationRepo.GetPendingByFamilyIdAsync(TestFamilyId, Arg.Any<CancellationToken>())
            .Returns(new List<FamilyInvitation> { invitation });

        var provider = CreateProvider(invitationRepo: invitationRepo);
        var context = new SearchContext(TestUserId, TestFamilyId, "alice");

        var results = await provider.SearchAsync(context);

        results.Should().Contain(r =>
            r.Title == "alice@example.com" &&
            r.Icon == "mail" &&
            r.Description == "Pending invitation");
    }

    [Fact]
    public async Task SearchAsync_InvitationWithGermanLocale()
    {
        var invitationRepo = Substitute.For<IFamilyInvitationRepository>();
        var invitation = FamilyInvitation.Create(
            TestFamilyId,
            TestUserId,
            Email.From("bob@example.com"),
            FamilyRole.Member,
            InvitationToken.From("b".PadRight(64, 'b')),
            "plaintext-token");
        invitation.ClearDomainEvents();

        invitationRepo.GetPendingByFamilyIdAsync(TestFamilyId, Arg.Any<CancellationToken>())
            .Returns(new List<FamilyInvitation> { invitation });

        var provider = CreateProvider(invitationRepo: invitationRepo);
        var context = new SearchContext(TestUserId, TestFamilyId, "bob", Locale: "de");

        var results = await provider.SearchAsync(context);

        results.Should().Contain(r => r.Description == "Ausstehende Einladung");
    }

    private static FamilySearchProvider CreateProvider(
        IFamilyMemberRepository? memberRepo = null,
        IFamilyRepository? familyRepo = null,
        IFamilyInvitationRepository? invitationRepo = null,
        FamilyId? familyId = null)
    {
        var effectiveFamilyId = familyId ?? TestFamilyId;

        if (memberRepo is null)
        {
            memberRepo = Substitute.For<IFamilyMemberRepository>();
            memberRepo.GetByFamilyIdAsync(effectiveFamilyId, Arg.Any<CancellationToken>())
                .Returns(new List<FamilyMember>());
        }

        if (invitationRepo is null)
        {
            invitationRepo = Substitute.For<IFamilyInvitationRepository>();
            invitationRepo.GetPendingByFamilyIdAsync(effectiveFamilyId, Arg.Any<CancellationToken>())
                .Returns(new List<FamilyInvitation>());
        }

        return new FamilySearchProvider(
            memberRepo,
            familyRepo ?? Substitute.For<IFamilyRepository>(),
            invitationRepo);
    }
}
