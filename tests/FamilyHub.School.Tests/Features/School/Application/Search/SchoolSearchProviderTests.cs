using System.Reflection;
using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Search;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.School.Tests.Features.School.Application.Search;

public class SchoolSearchProviderTests
{
    [Fact]
    public async Task SearchAsync_NullFamilyId_ReturnsEmpty()
    {
        var provider = CreateProvider([], []);
        var context = new SearchContext(UserId.New(), FamilyId: null, Query: "John");

        var result = await provider.SearchAsync(context);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsEmpty()
    {
        var provider = CreateProvider([], []);
        var context = new SearchContext(UserId.New(), FamilyId.New(), Query: "   ");

        var result = await provider.SearchAsync(context);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ExactNameMatch_ReturnsStudent()
    {
        var familyId = FamilyId.New();
        var (member, student) = CreateMemberWithStudent(familyId, "John Doe");

        var provider = CreateProvider([student], [member]);
        var context = new SearchContext(UserId.New(), familyId, Query: "John Doe");

        var result = await provider.SearchAsync(context);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("John Doe");
        result[0].Module.Should().Be("school");
        result[0].Icon.Should().Be("graduation-cap");
    }

    [Fact]
    public async Task SearchAsync_PartialMatch_ReturnsStudent()
    {
        var familyId = FamilyId.New();
        var (member, student) = CreateMemberWithStudent(familyId, "John Doe");

        var provider = CreateProvider([student], [member]);
        var context = new SearchContext(UserId.New(), familyId, Query: "Joh");

        var result = await provider.SearchAsync(context);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("John Doe");
    }

    [Fact]
    public async Task SearchAsync_FuzzyTypo_ReturnsStudent()
    {
        var familyId = FamilyId.New();
        var (member, student) = CreateMemberWithStudent(familyId, "John Doe");

        var provider = CreateProvider([student], [member]);
        var context = new SearchContext(UserId.New(), familyId, Query: "Jonn");

        var result = await provider.SearchAsync(context);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("John Doe");
    }

    [Fact]
    public async Task SearchAsync_NoMatch_ReturnsEmpty()
    {
        var familyId = FamilyId.New();
        var (member, student) = CreateMemberWithStudent(familyId, "John Doe");

        var provider = CreateProvider([student], [member]);
        var context = new SearchContext(UserId.New(), familyId, Query: "Xyz");

        var result = await provider.SearchAsync(context);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_RespectsLimit()
    {
        var familyId = FamilyId.New();
        var (member1, student1) = CreateMemberWithStudent(familyId, "Alice");
        var (member2, student2) = CreateMemberWithStudent(familyId, "Alicia");
        var (member3, student3) = CreateMemberWithStudent(familyId, "Alina");

        var provider = CreateProvider([student1, student2, student3], [member1, member2, member3]);
        var context = new SearchContext(UserId.New(), familyId, Query: "Ali", Limit: 2);

        var result = await provider.SearchAsync(context);

        result.Should().HaveCount(2);
    }

    // --- Helpers ---

    private static SchoolSearchProvider CreateProvider(
        List<Student> students,
        List<FamilyMember> members)
    {
        var familyId = students.FirstOrDefault()?.FamilyId ?? members.FirstOrDefault()?.FamilyId ?? FamilyId.New();
        var studentRepo = new FakeStudentRepository(students);
        var memberRepo = new FakeFamilyMemberRepository(allMembers: members);
        return new SchoolSearchProvider(studentRepo, memberRepo);
    }

    private static (FamilyMember Member, Student Student) CreateMemberWithStudent(
        FamilyId familyId,
        string name)
    {
        var userId = UserId.New();
        var member = FamilyMember.Create(familyId, userId, FamilyRole.Member);

        // Create a User with the given name and set it on the FamilyMember via reflection
        var user = User.Register(
            Email.From($"{name.Replace(" ", "").ToLowerInvariant()}@test.com"),
            UserName.From(name),
            ExternalUserId.From(Guid.NewGuid().ToString()),
            emailVerified: true);

        typeof(FamilyMember)
            .GetProperty("User")!
            .SetValue(member, user, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);

        var student = Student.Create(member.Id, familyId, UserId.New());
        return (member, student);
    }
}
