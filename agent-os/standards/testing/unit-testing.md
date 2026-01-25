# Unit Testing (xUnit)

Use xUnit with FluentAssertions and NSubstitute.

## Test Structure

```csharp
public class CreateFamilyCommandHandlerTests
{
    private readonly IFamilyRepository _repository;
    private readonly CreateFamilyCommandHandler _handler;

    public CreateFamilyCommandHandlerTests()
    {
        _repository = Substitute.For<IFamilyRepository>();
        _handler = new CreateFamilyCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesFamily()
    {
        // Arrange
        var command = new CreateFamilyCommand(FamilyName.From("Smith"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FamilyId.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(
            Arg.Is<Family>(f => f.Name.Value == "Smith"),
            Arg.Any<CancellationToken>());
    }
}
```

## AutoData Pattern

```csharp
public class AutoNSubstituteDataAttribute : AutoDataAttribute
{
    public AutoNSubstituteDataAttribute()
        : base(() => new Fixture().Customize(new AutoNSubstituteCustomization()))
    { }
}

[Theory, AutoNSubstituteData]
public async Task Handle_ValidCommand_CreatesFamily(
    [Frozen] IFamilyRepository repository,
    CreateFamilyCommandHandler handler,
    CreateFamilyCommand command)
{
    var result = await handler.Handle(command, CancellationToken.None);

    result.FamilyId.Should().NotBeEmpty();
}
```

## Rules

- Use FluentAssertions for readable assertions
- Use NSubstitute for mocking
- Arrange-Act-Assert pattern
- One assertion concept per test
- Location: `tests/FamilyHub.Tests.Unit/{Module}/`
