---
name: unit-test
description: Create xUnit unit test with FluentAssertions and NSubstitute
category: testing
module-aware: true
inputs:
  - testName: Test class name
  - classToTest: Class being tested
  - module: DDD module name
---

# Unit Test Skill

Create xUnit unit tests with FluentAssertions and NSubstitute for mocking.

## Context

Load module profile: `agent-os/profiles/modules/{module}.yaml`

## Steps

### 1. Create Test Class

**Location:** `Modules/FamilyHub.Modules.{Module}.Tests/Application/Commands/{CommandName}HandlerTests.cs`

```csharp
using FluentAssertions;
using NSubstitute;
using Xunit;
using FamilyHub.Modules.{Module}.Application.Commands.{CommandName};
using FamilyHub.Modules.{Module}.Domain.Entities;
using FamilyHub.Modules.{Module}.Domain.Repositories;
using FamilyHub.Modules.{Module}.Domain.ValueObjects;

namespace FamilyHub.Modules.{Module}.Tests.Application.Commands;

public class {CommandName}HandlerTests
{
    private readonly I{Entity}Repository _repositoryMock;
    private readonly {CommandName}Handler _sut;

    public {CommandName}HandlerTests()
    {
        _repositoryMock = Substitute.For<I{Entity}Repository>();
        _sut = new {CommandName}Handler(_repositoryMock);
    }

    [Fact]
    public async Task Handle_ValidCommand_Creates{Entity}()
    {
        // Arrange
        var command = new {CommandName}(
            {ValueObject}.From("test-value")
        );

        _repositoryMock
            .AddAsync(Arg.Any<{Entity}>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();

        await _repositoryMock.Received(1)
            .AddAsync(Arg.Any<{Entity}>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Arrange
        var command = new {CommandName}(
            {ValueObject}.From("")  // Invalid value
        );

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*cannot be empty*");
    }
}
```

### 2. Test Domain Entity

**Location:** `Modules/FamilyHub.Modules.{Module}.Tests/Domain/Entities/{Entity}Tests.cs`

```csharp
using FluentAssertions;
using Xunit;
using FamilyHub.Modules.{Module}.Domain.Entities;
using FamilyHub.Modules.{Module}.Domain.Events;
using FamilyHub.Modules.{Module}.Domain.ValueObjects;

namespace FamilyHub.Modules.{Module}.Tests.Domain.Entities;

public class {Entity}Tests
{
    [Fact]
    public void Create_ValidParameters_ReturnsEntity()
    {
        // Arrange
        var name = {EntityName}.From("Test Name");

        // Act
        var entity = {Entity}.Create(name);

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBeEmpty();
        entity.Name.Should().Be(name);
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ValidParameters_RaisesDomainEvent()
    {
        // Arrange
        var name = {EntityName}.From("Test Name");

        // Act
        var entity = {Entity}.Create(name);

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<{Entity}CreatedEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidName_ThrowsException(string? invalidName)
    {
        // Arrange & Act
        var act = () => {EntityName}.From(invalidName!);

        // Assert
        act.Should().Throw<ValueObjectValidationException>();
    }
}
```

### 3. Test Value Object

```csharp
public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.co.uk")]
    public void From_ValidEmail_ReturnsEmail(string validEmail)
    {
        // Act
        var email = Email.From(validEmail);

        // Assert
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("missing@")]
    public void From_InvalidEmail_ThrowsException(string invalidEmail)
    {
        // Act
        var act = () => Email.From(invalidEmail);

        // Assert
        act.Should().Throw<ValueObjectValidationException>();
    }
}
```

## Common Patterns

**Verify method called with specific argument:**

```csharp
await _repositoryMock.Received(1)
    .AddAsync(
        Arg.Is<{Entity}>(e => e.Name == expectedName),
        Arg.Any<CancellationToken>()
    );
```

**Return specific value from mock:**

```csharp
_repositoryMock.GetByIdAsync(Arg.Any<{EntityId}>(), Arg.Any<CancellationToken>())
    .Returns(expectedEntity);
```

**Throw exception from mock:**

```csharp
_repositoryMock.AddAsync(Arg.Any<{Entity}>(), Arg.Any<CancellationToken>())
    .ThrowsAsync(new Exception("Database error"));
```

## Validation

- [ ] Test class in Tests project under same namespace structure
- [ ] Uses FluentAssertions for all assertions
- [ ] Uses NSubstitute for mocking
- [ ] Follows Arrange-Act-Assert pattern
- [ ] Tests both happy path and error cases
