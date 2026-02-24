using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Services;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class OrganizationRuleEngineTests
{
    private readonly OrganizationRuleEngine _engine = new();
    private readonly FamilyId _familyId = FamilyId.New();
    private readonly UserId _userId = UserId.New();

    private StoredFile CreateFile(string name, string mimeType, long size)
    {
        return StoredFile.Create(
            FileName.From(name),
            MimeType.From(mimeType),
            FileSize.From(size),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(),
            _familyId,
            _userId);
    }

    private OrganizationRule CreateRule(
        string conditionsJson,
        ConditionLogic logic = ConditionLogic.And,
        RuleActionType actionType = RuleActionType.MoveToFolder,
        int priority = 1,
        bool enabled = true)
    {
        var rule = OrganizationRule.Create(
            $"Rule {priority}",
            _familyId,
            _userId,
            conditionsJson,
            logic,
            actionType,
            """{"DestinationFolderId":"00000000-0000-0000-0000-000000000001"}""",
            priority);

        if (!enabled) rule.Disable();
        return rule;
    }

    [Fact]
    public void EvaluateFile_ShouldMatchFileExtension()
    {
        var file = CreateFile("photo.jpg", "image/jpeg", 1024);
        var rules = new List<OrganizationRule>
        {
            CreateRule("""[{"Type":1,"Value":".jpg,.png"}]""")
        };

        var result = _engine.EvaluateFile(file, rules);

        result.Should().NotBeNull();
        result!.Matched.Should().BeTrue();
    }

    [Fact]
    public void EvaluateFile_ShouldMatchMimeType()
    {
        var file = CreateFile("photo.jpg", "image/jpeg", 1024);
        var rules = new List<OrganizationRule>
        {
            CreateRule("""[{"Type":2,"Value":"image/*"}]""")
        };

        var result = _engine.EvaluateFile(file, rules);

        result.Should().NotBeNull();
        result!.Matched.Should().BeTrue();
    }

    [Fact]
    public void EvaluateFile_ShouldMatchExactMimeType()
    {
        var file = CreateFile("doc.pdf", "application/pdf", 1024);
        var rules = new List<OrganizationRule>
        {
            CreateRule("""[{"Type":2,"Value":"application/pdf"}]""")
        };

        var result = _engine.EvaluateFile(file, rules);

        result.Should().NotBeNull();
        result!.Matched.Should().BeTrue();
    }

    [Fact]
    public void EvaluateFile_ShouldMatchFileNameRegex()
    {
        var file = CreateFile("invoice-2024-01.pdf", "application/pdf", 1024);
        var rules = new List<OrganizationRule>
        {
            CreateRule("""[{"Type":3,"Value":"invoice-\\d{4}"}]""")
        };

        var result = _engine.EvaluateFile(file, rules);

        result.Should().NotBeNull();
        result!.Matched.Should().BeTrue();
    }

    [Fact]
    public void EvaluateFile_ShouldMatchSizeGreaterThan()
    {
        var file = CreateFile("large.zip", "application/zip", 10_000_000);
        var rules = new List<OrganizationRule>
        {
            CreateRule("""[{"Type":4,"Value":"5000000"}]""")
        };

        var result = _engine.EvaluateFile(file, rules);

        result.Should().NotBeNull();
        result!.Matched.Should().BeTrue();
    }

    [Fact]
    public void EvaluateFile_ShouldMatchSizeLessThan()
    {
        var file = CreateFile("small.txt", "text/plain", 100);
        var rules = new List<OrganizationRule>
        {
            CreateRule("""[{"Type":5,"Value":"1000"}]""")
        };

        var result = _engine.EvaluateFile(file, rules);

        result.Should().NotBeNull();
        result!.Matched.Should().BeTrue();
    }

    [Fact]
    public void EvaluateFile_ShouldNotMatchWhenNoConditionsMet()
    {
        var file = CreateFile("photo.jpg", "image/jpeg", 1024);
        var rules = new List<OrganizationRule>
        {
            CreateRule("""[{"Type":1,"Value":".pdf"}]""")
        };

        var result = _engine.EvaluateFile(file, rules);

        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateFile_ShouldReturnNullWhenNoRules()
    {
        var file = CreateFile("photo.jpg", "image/jpeg", 1024);

        var result = _engine.EvaluateFile(file, []);

        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateFile_ShouldSkipDisabledRules()
    {
        var file = CreateFile("photo.jpg", "image/jpeg", 1024);
        var rules = new List<OrganizationRule>
        {
            CreateRule("""[{"Type":1,"Value":".jpg"}]""", enabled: false)
        };

        var result = _engine.EvaluateFile(file, rules);

        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateFile_AndLogic_ShouldRequireAllConditions()
    {
        var file = CreateFile("photo.jpg", "image/jpeg", 1024);
        var rules = new List<OrganizationRule>
        {
            CreateRule(
                """[{"Type":1,"Value":".jpg"},{"Type":4,"Value":"5000000"}]""",
                ConditionLogic.And)
        };

        // File is .jpg but only 1024 bytes, not > 5MB
        var result = _engine.EvaluateFile(file, rules);

        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateFile_OrLogic_ShouldMatchIfAnyConditionMet()
    {
        var file = CreateFile("photo.jpg", "image/jpeg", 1024);
        var rules = new List<OrganizationRule>
        {
            CreateRule(
                """[{"Type":1,"Value":".jpg"},{"Type":4,"Value":"5000000"}]""",
                ConditionLogic.Or)
        };

        // File is .jpg (first condition matches), even though < 5MB
        var result = _engine.EvaluateFile(file, rules);

        result.Should().NotBeNull();
        result!.Matched.Should().BeTrue();
    }

    [Fact]
    public void EvaluateFile_ShouldReturnFirstMatchByPriority()
    {
        var file = CreateFile("photo.jpg", "image/jpeg", 1024);
        var rules = new List<OrganizationRule>
        {
            CreateRule("""[{"Type":1,"Value":".jpg"}]""", priority: 2),
            CreateRule("""[{"Type":2,"Value":"image/*"}]""", priority: 1)
        };

        // Sort by priority (lower first)
        var sorted = rules.OrderBy(r => r.Priority).ToList();
        var result = _engine.EvaluateFile(file, sorted);

        result.Should().NotBeNull();
        result!.MatchedRuleName.Should().Be("Rule 1");
    }

    [Fact]
    public void EvaluateFile_ShouldHandleInvalidRegexGracefully()
    {
        var file = CreateFile("test.txt", "text/plain", 100);
        var rules = new List<OrganizationRule>
        {
            CreateRule("""[{"Type":3,"Value":"[invalid"}]""")
        };

        var result = _engine.EvaluateFile(file, rules);

        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateFile_ShouldHandleInvalidConditionsJsonGracefully()
    {
        var rule = OrganizationRule.Create(
            "Bad rule", _familyId, _userId,
            "not-json",
            ConditionLogic.And,
            RuleActionType.MoveToFolder,
            "{}",
            1);

        var file = CreateFile("test.txt", "text/plain", 100);
        var result = _engine.EvaluateFile(file, [rule]);

        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateFile_ShouldMatchExtensionWithoutDot()
    {
        var file = CreateFile("photo.jpg", "image/jpeg", 1024);
        var rules = new List<OrganizationRule>
        {
            CreateRule("""[{"Type":1,"Value":"jpg"}]""")
        };

        var result = _engine.EvaluateFile(file, rules);

        result.Should().NotBeNull();
        result!.Matched.Should().BeTrue();
    }

    [Fact]
    public void EvaluateFile_AndLogic_ShouldMatchWhenAllConditionsMet()
    {
        var file = CreateFile("photo.jpg", "image/jpeg", 10_000_000);
        var rules = new List<OrganizationRule>
        {
            CreateRule(
                """[{"Type":1,"Value":".jpg"},{"Type":4,"Value":"5000000"}]""",
                ConditionLogic.And)
        };

        var result = _engine.EvaluateFile(file, rules);

        result.Should().NotBeNull();
        result!.Matched.Should().BeTrue();
    }
}
