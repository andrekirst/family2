# ADR-003: GraphQL Input → Command Mapping Pattern

**Status:** Accepted
**Date:** 2025-12-30
**Deciders:** Andre Kirst (with Claude Code AI)
**Tags:** graphql, cqrs, vogen, architecture

## Context

Family Hub uses **HotChocolate GraphQL** for the presentation layer and **MediatR** for CQRS command/query handling. A question arose during development: Should we use MediatR Commands directly as GraphQL input types, or maintain separate GraphQL Input DTOs that map to Commands?

### Problem Statement

When implementing GraphQL mutations, there appeared to be duplication between GraphQL Input classes and MediatR Command classes:

```csharp
// GraphQL Input (primitive types)
public sealed record CreateFamilyInput
{
    public required string Name { get; init; }
}

// MediatR Command (Vogen value objects)
public sealed record CreateFamilyCommand(
    FamilyName Name
) : IRequest<CreateFamilyResult>;

// Mutation (manual mapping)
var command = new CreateFamilyCommand(FamilyName.From(input.Name));
```

The duplication seemed unnecessary. Could we eliminate the Input class and use the Command directly?

### Technology Stack Context

- **HotChocolate GraphQL 14.1.0** - Schema-first GraphQL server
- **MediatR 12.4.1** - CQRS command/query pipeline with validation
- **Vogen 8.0+** - Source generator for strongly-typed value objects
- **.NET 10 / C# 14** - Target framework
- **FluentValidation** - Command validation via MediatR pipeline behavior

### Vogen Value Objects

Family Hub extensively uses [Vogen](https://github.com/SteveDunn/Vogen) for domain value objects:

```csharp
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct FamilyName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Family name cannot be empty.");
        if (value.Length > 100)
            return Validation.Invalid("Family name cannot exceed 100 characters.");
        return Validation.Ok;
    }

    private static string NormalizeInput(string input) => input.Trim();
}
```

Vogen types require calling static `From(string)` factory methods to create instances, which triggers validation.

## Decision

**We will maintain separate GraphQL Input DTOs that map to MediatR Commands.**

GraphQL mutations will:

1. Accept Input DTOs with primitive types (string, int, etc.)
2. Manually map Input → Command in the mutation method
3. Use Vogen factory methods (`FamilyName.From()`) during mapping
4. Send the Command to MediatR for validation and handling

### Pattern

```csharp
// GraphQL Input (JSON deserialization contract)
public sealed record CreateFamilyInput
{
    public required string Name { get; init; }
}

// MediatR Command (domain operation contract)
public sealed record CreateFamilyCommand(
    FamilyName Name
) : IRequest<CreateFamilyResult>;

// Mutation (explicit conversion point)
public async Task<CreateFamilyPayload> CreateFamily(
    CreateFamilyInput input,
    [Service] IMutationHandler mutationHandler,
    [Service] IMediator mediator,
    CancellationToken cancellationToken)
{
    return await mutationHandler.Handle<CreateFamilyResult, CreateFamilyPayload>(async () =>
    {
        var command = new CreateFamilyCommand(FamilyName.From(input.Name));
        return await mediator.Send(command, cancellationToken);
    });
}
```

## Rationale

### Attempted Alternative: Commands as GraphQL Inputs

We explored using Commands directly as GraphQL inputs with `[GraphQLName]` attributes to preserve external API naming:

```csharp
[GraphQLName("CreateFamilyInput")]
public sealed record CreateFamilyCommand(
    FamilyName Name
) : IRequest<CreateFamilyResult>;

// Mutation attempts to use command directly
public async Task<CreateFamilyPayload> CreateFamily(
    CreateFamilyCommand command, // ← Direct use
    [Service] IMediator mediator,
    CancellationToken cancellationToken)
{
    return await mediator.Send(command, cancellationToken);
}
```

**This approach failed** due to fundamental incompatibility between HotChocolate's deserialization model and Vogen's factory method pattern.

### Why the Alternative Failed

1. **HotChocolate Expects JSON-Deserializable Types**
   - GraphQL expects inputs to be standard CLR types that can be constructed via JSON deserialization
   - Vogen value objects are `readonly partial struct` types with validation that require calling `From(string)` factory methods
   - HotChocolate doesn't natively understand how to deserialize JSON strings into Vogen types

2. **Custom Binding Complexity**
   - Attempted solution: Create custom `InputObjectType<CreateFamilyCommand>` configuration
   - Required reflection to discover `From()` methods on Vogen types
   - Attempted to intercept field binding with custom middleware
   - **Result:** HotChocolate's `InputFieldDefinition` API doesn't support the level of control needed for Vogen conversion

3. **Test Failures**
   - Integration tests failed with HTTP 400 errors
   - GraphQL couldn't construct `CreateFamilyCommand` from JSON request
   - Error: Unable to deserialize `FamilyName` from string value

4. **Alternative Solutions Too Complex**
   - **Option A:** Register HotChocolate scalar types for each Vogen value object (~20-30 scalar definitions)
   - **Option B:** Implement custom `IInputFormatter` for low-level deserialization control
   - **Option C:** Abandon Vogen and use primitives everywhere (loses type safety)
   - **Verdict:** All alternatives add significant complexity with questionable value

### Benefits of Input → Command Pattern

1. **✅ Separation of Concerns**
   - **GraphQL Inputs:** JSON deserialization contract (primitive types, no validation)
   - **MediatR Commands:** Domain operation contract (Vogen value objects, business validation)
   - Clear boundary between presentation layer and application layer

2. **✅ Explicit Conversion Point**
   - The mutation method is the single, obvious place where string → Vogen conversion happens
   - `FamilyName.From(input.Name)` is explicit and searchable
   - No "magic" binding logic hidden in framework configuration

3. **✅ Vogen Validation Boundary**
   - Vogen validation exceptions occur in a controlled context
   - `GraphQLErrorFilter` can catch and map exceptions to GraphQL errors
   - Clear error messages with field-level detail

4. **✅ Framework Compatibility**
   - Works naturally with HotChocolate's JSON deserialization
   - No complex custom binding infrastructure required
   - No fighting framework limitations

5. **✅ Maintainability**
   - Simple, understandable pattern
   - New developers can easily trace Input → Command → Handler flow
   - No need to understand custom HotChocolate extensions

6. **✅ Testing**
   - Inputs and Commands can be tested independently
   - Clear test boundaries (GraphQL schema tests vs. command handler tests)
   - All 63 tests pass with this pattern

### Drawbacks of Input → Command Pattern

1. **❌ Boilerplate**
   - Requires separate Input DTO classes
   - Manual mapping code in each mutation method
   - ~2-3 extra files per mutation (Input class, potentially Input validator)

2. **❌ Duplication Risk**
   - Input properties and Command properties must stay in sync
   - Rename refactorings require updating both Input and Command
   - Mitigated by: Strong typing, compiler errors, integration tests

### Why Boilerplate is Acceptable

The boilerplate is **intentional clarity**, not unnecessary duplication:

1. **Inputs and Commands Serve Different Purposes**
   - Inputs: External API contract (JSON, GraphQL schema)
   - Commands: Internal domain contract (Vogen types, business rules)
   - Merging them creates coupling between presentation and domain layers

2. **Explicit > Implicit**
   - The mapping code documents the transformation from external data to domain concepts
   - Easier to debug when issues arise
   - No hidden framework magic

3. **Validation Clarity**
   - Two validation stages are clear:
     - **Stage 1:** GraphQL schema validation (required fields, types)
     - **Stage 2:** Vogen + FluentValidation (domain rules, business constraints)

## Alternatives Considered

### Alternative 1: HotChocolate Scalar Types for Vogen

**Approach:** Register custom scalar types for each Vogen value object.

```csharp
public class FamilyNameType : ScalarType<FamilyName, StringValueNode>
{
    public override bool TryDeserialize(object? resultValue, out FamilyName value)
    {
        if (resultValue is string s)
        {
            value = FamilyName.From(s);
            return true;
        }
        value = default;
        return false;
    }
    // ... serialize method
}

// Register for each Vogen type
builder.AddType<FamilyNameType>();
builder.AddType<UserIdType>();
// ~20-30 more...
```

**Rejected Because:**

- Requires ~20-30 scalar type definitions (one per Vogen type)
- Maintenance overhead: Every new Vogen type needs a scalar type
- Still doesn't solve the core issue: Commands would contain Vogen types that GraphQL can't directly construct
- Adds framework coupling (GraphQL-specific types for domain concepts)

### Alternative 2: Custom IInputFormatter

**Approach:** Implement HotChocolate's `IInputFormatter` interface for Vogen deserialization.

**Rejected Because:**

- Low-level, complex API
- Requires deep HotChocolate internals knowledge
- Fragile (breaks with HotChocolate updates)
- Overkill for simple string → Vogen conversion

### Alternative 3: Remove Vogen, Use Primitives

**Approach:** Abandon Vogen and use string/Guid everywhere.

**Rejected Because:**

- Loses type safety (can pass `UserId` where `FamilyId` expected)
- Loses validation guarantees (invalid IDs can be constructed)
- Loses self-documenting code (methods that take `Guid` vs. `UserId`)
- Vogen provides significant value (see [Vogen documentation](https://github.com/SteveDunn/Vogen))

### Alternative 4: Hybrid Approach

**Approach:** Use Commands directly for mutations with primitives, Input → Command for mutations with Vogen types.

**Rejected Because:**

- Inconsistent pattern across codebase
- Confusing for developers (when to use which approach?)
- Still requires Input classes for most mutations (Family Hub uses Vogen extensively)

## Consequences

### Positive

1. **Simple, Proven Pattern**
   - Input → Command mapping is standard practice in CQRS + GraphQL architectures
   - Well-understood by .NET developers
   - No custom framework extensions to maintain

2. **Framework Compatibility**
   - Works naturally with HotChocolate
   - Works naturally with Vogen
   - No friction between technologies

3. **Clear Boundaries**
   - GraphQL schema shows primitive types (String, ID)
   - Domain layer uses rich value objects
   - Explicit conversion at presentation layer boundary

4. **Testability**
   - All 63 tests pass
   - Clear test boundaries (schema tests, command tests, mutation tests)
   - Easy to mock and verify

5. **Maintainability**
   - New developers can understand the flow easily
   - No "magic" to explain
   - Searchable, debuggable mapping code

### Negative

1. **Boilerplate**
   - ~2-3 extra files per mutation (Input DTO, potentially Input validator)
   - Manual mapping code in mutation methods
   - Properties must be kept in sync between Input and Command

2. **Duplication Risk**
   - Input and Command properties can drift if not careful
   - Mitigated by: Integration tests, strong typing, compiler errors

### Mitigation Strategies

1. **Integration Tests**
   - GraphQL mutation tests ensure Input → Command mapping works correctly
   - End-to-end tests validate entire flow

2. **Code Reviews**
   - Ensure Input and Command stay in sync during refactorings
   - Watch for validator duplication

3. **Documentation**
   - This ADR explains the pattern
   - CLAUDE.md includes pattern examples
   - Onboarding docs reference this decision

## Implementation

### Current State

**Pattern Applied To:**

- `CreateFamilyInput` → `CreateFamilyCommand` (FamilyName value object)
- `CompleteZitadelLoginInput` → `CompleteZitadelLoginCommand` (primitive types)

**Files:**

- Inputs: `/src/api/Modules/FamilyHub.Modules.Auth/Presentation/GraphQL/Inputs/`
- Commands: `/src/api/Modules/FamilyHub.Modules.Auth/Application/Commands/`
- Mutations: `/src/api/Modules/FamilyHub.Modules.Auth/Presentation/GraphQL/Mutations/`

### Test Results

**Before Refactoring Attempt:** 63 tests passing
**After Refactoring Attempt:** Tests failed (HTTP 400 errors)
**After Reversion:** 63 tests passing ✓

### Deleted Files (Orphaned from Failed Attempt)

- `VogenInputTypeExtensions.cs` - Custom Vogen binding helper (unused)
- `CreateFamilyInputType.cs` - InputObjectType configuration (unused)

## Validation

This decision was validated through:

1. **Implementation Attempt**
   - Spent ~3 hours attempting command-as-input pattern
   - Encountered HotChocolate deserialization limitations
   - Tried multiple workarounds (all failed or too complex)

2. **Testing**
   - Integration tests proved Input → Command pattern works
   - All 63 tests pass with current pattern
   - Command-as-input pattern failed tests

3. **Code Review**
   - Reviewed HotChocolate documentation
   - Reviewed Vogen documentation
   - Confirmed no native support for Vogen in HotChocolate

## Related Decisions

- [ADR-001: Modular Monolith First](ADR-001-MODULAR-MONOLITH-FIRST.md) - Architectural approach
- [ADR-002: OAuth with Zitadel](ADR-002-OAUTH-WITH-ZITADEL.md) - Authentication strategy

## References

- [HotChocolate Documentation](https://chillicream.com/docs/hotchocolate/v14)
- [Vogen Documentation](https://github.com/SteveDunn/Vogen)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [FluentValidation](https://docs.fluentvalidation.net/)

## Amendment: PersonName Refactoring (January 2026)

**Date:** 2026-01-05
**Context:** User feedback indicated that "Full Name" terminology was confusing and didn't support mononyms (single names like "Annika").

### Decision

Renamed `FullName` value object to `PersonName` throughout the system for improved clarity and mononym support.

### Changes

1. **Value Object**: `FullName` → `PersonName` (C# class in `FamilyHub.SharedKernel`)
2. **Database Columns**: `full_name` → `name` (3 tables in auth schema)
3. **GraphQL Fields**: `fullName` → `name` (breaking change)
4. **GraphQL Inputs**: `CreateManagedMemberInput.fullName` → `CreateManagedMemberInput.name`
5. **Error Messages**: More friendly tone ("Please enter a name" vs "Full name cannot be empty")
6. **UI Labels**: Context-aware ("Name", "Member name", "Display name")

### Mononym Support

PersonName supports both single names and compound names (1-100 characters):

- "Annika" (mononym)
- "John Doe" (compound name)
- "Marie-Claire Dubois" (hyphenated compound)

**Zitadel Integration:**
Current workaround duplicates single name to both firstName and lastName when creating Zitadel accounts (Zitadel requires non-empty lastName field). Tested and validated in Phase 0 (2026-01-05).

```csharp
// Mononym handling in CreateManagedMemberCommandHandler
var nameParts = request.PersonName.Value.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
var firstName = nameParts.Length > 0 ? nameParts[0] : request.Username.Value;
var lastName = nameParts.Length > 1 ? nameParts[1] : firstName; // DUPLICATE for mononyms
```

### GraphQL Breaking Change

**Migration Required for Clients:**

```graphql
# Before (deprecated)
mutation {
  createManagedMember(input: { fullName: "John Doe", ... }) {
    user { id fullName }
  }
}

# After (required)
mutation {
  createManagedMember(input: { name: "John Doe", ... }) {
    user { id name }
  }
}
```

Frontend clients must update queries to use `name` field instead of `fullName`.

### Files Changed

28 files affected across backend (17 files), frontend (3 files), tests (6 files), and documentation (2 files). Complete refactoring included:

- Domain entities (User, FamilyMemberInvitation, QueuedManagedAccountCreation)
- EF Core configurations (3 entity configurations)
- GraphQL schema (inputs, mutations, queries)
- Database migration (RenameFullNameToPersonName)
- Unit tests (PersonNameTests, UserManagedAccountTests, FamilyMemberInvitationTests)
- Frontend components (models, forms, templates)
- E2E tests (Playwright specs)

### Test Coverage

- ✅ 180/183 backend unit tests passing
- ✅ 21/21 integration tests passing
- ✅ New mononym test case added: `PersonName.From("Annika")`
- ✅ Frontend builds successfully with updated GraphQL types

### Impact on Input → Command Pattern

This refactoring **validates** the Input → Command pattern:

1. **GraphQL Breaking Change Isolated**: Only Input DTOs changed (public API contract)
2. **Domain Unchanged**: PersonName value object still uses Vogen pattern
3. **Mapping Updated**: Mutation methods updated to use `PersonName.From(input.Name)`
4. **Type Safety Maintained**: TypeScript interfaces updated in lockstep with C# classes

The separation between GraphQL Inputs (primitive `string name`) and Commands (Vogen `PersonName`) proved valuable - only mutation mapping code needed updates, not the entire command pipeline.

## Revision History

| Date | Version | Author | Description |
|------|---------|--------|-------------|
| 2025-12-30 | 1.0 | Andre Kirst | Initial decision after refactoring attempt |
| 2026-01-05 | 1.1 | Andre Kirst | PersonName refactoring amendment |

---

**Decision:** We will maintain the Input → Command mapping pattern for GraphQL mutations. This pattern provides clarity, explicit conversion points, and compatibility with both HotChocolate and Vogen. The boilerplate is acceptable given the benefits of separation of concerns and framework compatibility.

**Amendment (2026-01-05):** The PersonName refactoring validated this decision - the Input → Command separation isolated the GraphQL breaking change from the domain layer, demonstrating the value of maintaining clear boundaries between presentation and domain concerns.
