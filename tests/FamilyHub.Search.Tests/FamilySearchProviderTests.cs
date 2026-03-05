using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Family.Application.Search;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;
using FamilyHub.Api.Features.Family.Domain.Entities;

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
        var familyRepo = new FakeFamilyRepository();
        var family = FamilyEntity.Create(FamilyName.From("Smith Family"), TestUserId);
        family.ClearDomainEvents();
        familyRepo.Seed(family);

        var provider = CreateProvider(familyRepo: familyRepo,
            familyId: family.Id);
        var context = new SearchContext(TestUserId, family.Id, "smith");

        var results = await provider.SearchAsync(context);

        results.Should().Contain(r => r.Title == "Smith Family" && r.Icon == "home");
    }

    [Fact]
    public async Task SearchAsync_MatchesFamilyName_GermanLocale()
    {
        var familyRepo = new FakeFamilyRepository();
        var family = FamilyEntity.Create(FamilyName.From("Müller Familie"), TestUserId);
        family.ClearDomainEvents();
        familyRepo.Seed(family);

        var provider = CreateProvider(familyRepo: familyRepo,
            familyId: family.Id);
        var context = new SearchContext(TestUserId, family.Id, "müller", Locale: "de");

        var results = await provider.SearchAsync(context);

        results.Should().Contain(r => r.Title == "Müller Familie" && r.Description == "Deine Familie");
    }

    [Fact]
    public async Task SearchAsync_MatchesPendingInvitations()
    {
        var invitationRepo = new FakeFamilyInvitationRepository();
        var invitation = FamilyInvitation.Create(
            TestFamilyId,
            TestUserId,
            Email.From("alice@example.com"),
            FamilyRole.Member,
            InvitationToken.From("a".PadRight(64, 'a')),
            "plaintext-token");
        invitation.ClearDomainEvents();
        invitationRepo.Seed(invitation);

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
        var invitationRepo = new FakeFamilyInvitationRepository();
        var invitation = FamilyInvitation.Create(
            TestFamilyId,
            TestUserId,
            Email.From("bob@example.com"),
            FamilyRole.Member,
            InvitationToken.From("b".PadRight(64, 'b')),
            "plaintext-token");
        invitation.ClearDomainEvents();
        invitationRepo.Seed(invitation);

        var provider = CreateProvider(invitationRepo: invitationRepo);
        var context = new SearchContext(TestUserId, TestFamilyId, "bob", Locale: "de");

        var results = await provider.SearchAsync(context);

        results.Should().Contain(r => r.Description == "Ausstehende Einladung");
    }

    private static FamilySearchProvider CreateProvider(
        FakeFamilyMemberRepository? memberRepo = null,
        FakeFamilyRepository? familyRepo = null,
        FakeFamilyInvitationRepository? invitationRepo = null,
        FamilyId? familyId = null) =>
        new(
            memberRepo ?? new FakeFamilyMemberRepository(),
            familyRepo ?? new FakeFamilyRepository(),
            invitationRepo ?? new FakeFamilyInvitationRepository());
}
