using FamilyHub.Api.Common.Infrastructure.Behaviors;
using FamilyHub.Common.Application;
using FluentAssertions;

namespace FamilyHub.Auth.Tests.Common.Infrastructure.Behaviors;

public class InputSanitizationBehaviorTests
{
    #region Test message types

    private class TestCommand : FamilyHub.Common.Application.ICommand<string>
    {
        public string Name { get; set; } = "";
    }

    private class NestedAddress
    {
        public string Street { get; set; } = "";
        public string City { get; set; } = "";
    }

    private class TestCommandWithNested : FamilyHub.Common.Application.ICommand<string>
    {
        public string Name { get; set; } = "";
        public NestedAddress Address { get; set; } = new();
    }

    private class TestCommandWithList : FamilyHub.Common.Application.ICommand<string>
    {
        public string Name { get; set; } = "";
        public List<NestedAddress> Addresses { get; set; } = [];
    }

    private class TestQuery : IReadOnlyQuery<string>;

    #endregion

    private static ValueTask<string> SuccessHandler<TMessage>(TMessage _, CancellationToken __) =>
        new("handler-executed");

    [Fact]
    public async Task Should_sanitize_top_level_string_property()
    {
        var behavior = new InputSanitizationBehavior<TestCommand, string>();
        var command = new TestCommand { Name = "<script>alert(1)</script>Hello" };

        await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        command.Name.Should().NotContain("<script>");
        command.Name.Should().Contain("Hello");
    }

    [Fact]
    public async Task Should_sanitize_nested_object_string_property()
    {
        var behavior = new InputSanitizationBehavior<TestCommandWithNested, string>();
        var command = new TestCommandWithNested
        {
            Name = "Clean",
            Address = new NestedAddress
            {
                Street = "123 Main St<script>alert(1)</script>",
                City = "Springfield",
            },
        };

        await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        command.Address.Street.Should().NotContain("<script>");
        command.Address.Street.Should().Contain("123 Main St");
        command.Address.City.Should().Be("Springfield");
    }

    [Fact]
    public async Task Should_sanitize_string_properties_in_list_items()
    {
        var behavior = new InputSanitizationBehavior<TestCommandWithList, string>();
        var command = new TestCommandWithList
        {
            Name = "Clean",
            Addresses =
            [
                new NestedAddress { Street = "123 <b>Bold</b> Main St", City = "Safe City" },
                new NestedAddress { Street = "Normal St", City = "Good <em>bad</em> City" },
            ],
        };

        await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        command.Addresses[0].Street.Should().NotContain("<b>");
        command.Addresses[0].Street.Should().Contain("Main St");
        command.Addresses[1].City.Should().NotContain("<em>");
        command.Addresses[1].City.Should().Contain("City");
        command.Addresses[1].Street.Should().Be("Normal St", "clean strings in list items should be unchanged");
    }

    [Fact]
    public async Task Should_not_modify_clean_strings()
    {
        var behavior = new InputSanitizationBehavior<TestCommand, string>();
        var command = new TestCommand { Name = "John Doe" };

        await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        command.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task Should_skip_queries()
    {
        var behavior = new InputSanitizationBehavior<TestQuery, string>();
        var query = new TestQuery();

        var result = await behavior.Handle(query, SuccessHandler, CancellationToken.None);

        result.Should().Be("handler-executed");
    }

    [Fact]
    public async Task Should_handle_null_nested_object_without_error()
    {
        var behavior = new InputSanitizationBehavior<TestCommandWithNested, string>();
        var command = new TestCommandWithNested
        {
            Name = "<div>test</div>",
            Address = null!,
        };

        // Should not throw
        await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        command.Name.Should().NotContain("<div>");
    }
}
