# Vogen Value Objects

This folder contains value objects created using [Vogen](https://github.com/SteveDunn/Vogen), a .NET source generator for strongly-typed value objects.

## What is Vogen?

Vogen transforms primitive types into strongly-typed value objects, enforcing domain concepts and preventing invalid states through compile-time errors. It eliminates boilerplate code by automatically generating:

- Constructors and factory methods (`From`, `TryFrom`)
- Equality and comparison operators
- Validation logic
- Serialization converters (JSON, EF Core, Dapper, etc.)
- Type-safe operations

## Creating Value Objects

### Basic Value Object

```csharp
using Vogen;

[ValueObject<int>]
public readonly partial struct CustomerId;
```

### Value Object with Validation

```csharp
using Vogen;

[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Email cannot be empty.");
        }

        if (!IsValidEmailFormat(value))
        {
            return Validation.Invalid("Email format is invalid.");
        }

        return Validation.Ok;
    }

    private static string NormalizeInput(string input) =>
        input.Trim().ToLowerInvariant();
}
```

### Strongly-Typed IDs

```csharp
using Vogen;

[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct UserId
{
    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Validation.Invalid("UserId cannot be empty.");
        }
        return Validation.Ok;
    }

    public static UserId New() => From(Guid.NewGuid());
}
```

## Using Value Objects

### Creating Instances

```csharp
// Using From() method
var email = Email.From("user@example.com");

// Safe creation with TryFrom()
if (Email.TryFrom("user@example.com", out var validEmail))
{
    // Use validEmail
}

// Generate new ID
var userId = UserId.New();
```

### Accessing the Underlying Value

```csharp
var email = Email.From("user@example.com");
string emailString = email.Value; // "user@example.com"
```

## Entity Framework Core Integration

Vogen automatically generates EF Core value converters when you include `Conversions.EfCoreValueConverter` in the `conversions` parameter.

### Configuring in DbContext

```csharp
public class FamilyHubDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Option 1: Manual configuration
        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .HasConversion(new UserId.EfCoreValueConverter());

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .HasConversion(new Email.EfCoreValueConverter());

        // Option 2: Auto-discovery (recommended)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FamilyHubDbContext).Assembly);
    }
}
```

### Entity Configuration

```csharp
public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        // UserId is stored as GUID in database
        builder.Property(u => u.Id)
            .HasConversion(new UserId.EfCoreValueConverter())
            .HasColumnName("id");

        // Email is stored as string in database
        builder.Property(u => u.Email)
            .HasConversion(new Email.EfCoreValueConverter())
            .HasMaxLength(320)
            .HasColumnName("email");
    }
}
```

### Using in Entities

```csharp
public class User : AggregateRoot<UserId>
{
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }

    private User(UserId id, Email email) : base(id)
    {
        Email = email;
    }

    public static User Create(Email email, string passwordHash)
    {
        var user = new User(UserId.New(), email)
        {
            PasswordHash = passwordHash
        };

        user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Email));
        return user;
    }
}
```

## Benefits of Using Vogen

1. **Type Safety**: Prevents accidental assignment of wrong types
   ```csharp
   // Compile error: Cannot implicitly convert UserId to FamilyId
   UserId userId = UserId.New();
   FamilyId familyId = userId; // ❌ Compile error
   ```

2. **Domain Validation**: Ensures only valid values exist
   ```csharp
   var email = Email.From("invalid"); // ❌ Throws ValueObjectValidationException
   ```

3. **Self-Documenting Code**: Makes intent clear
   ```csharp
   // Before: What does this Guid represent?
   public void AssignTask(Guid id) { }

   // After: Clear intent
   public void AssignTask(UserId userId) { }
   ```

4. **Reduced Boilerplate**: No need to write equality, comparison, validation code

5. **EF Core Integration**: Seamless database mapping with automatic value converters

## Examples in This Project

- **Email**: Email address with regex validation and normalization
- **UserId**: Strongly-typed user identifier (Guid)
- **FamilyId**: Strongly-typed family identifier (Guid)

## References

- [Vogen GitHub Repository](https://github.com/SteveDunn/Vogen)
- [Vogen Documentation](https://github.com/SteveDunn/Vogen/tree/main/docs)
- [Entity Framework Core Value Converters](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions)

## Best Practices

1. **Always use `readonly partial struct`** for value objects (better performance than classes)
2. **Include `Conversions.EfCoreValueConverter`** when using with Entity Framework Core
3. **Implement validation** for domain-critical value objects
4. **Use normalization** to ensure consistent data format
5. **Provide factory methods** (like `New()`) for ID generation
6. **Keep value objects immutable** - no setters, only init properties
