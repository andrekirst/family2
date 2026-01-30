# ADR-013: GraphQL Schema Refactoring - Nested Namespaces and Relay Patterns

**Status:** Accepted
**Date:** 2026-01-28
**Deciders:** Andre Kirst (with Claude Code AI)
**Tags:** graphql, relay, namespaces, error-handling, subscriptions
**Supersedes:** Extends ADR-003 (does not replace)

## Context

As Family Hub's GraphQL schema grew with multiple modules (Auth, Family, UserProfile), several pain points emerged:

### Problems Identified

1. **Flat schema** - All queries/mutations at root level, hard to discover and organize
2. **Inconsistent naming** - Mixed `*Extension` suffix usage
3. **Three error patterns** - PayloadError, UserError, GraphQLException used inconsistently
4. **No Relay patterns** - No Node interface, Connections, or global IDs
5. **Cross-module violations** - Extensions in wrong modules
6. **Unclear field visibility** - No standard approach for sensitive fields

### Technology Stack

- **HotChocolate GraphQL 14.1.0** - Schema-first GraphQL server
- **Redis** - GraphQL subscriptions transport
- **MediatR 12.4.1** - CQRS command/query pipeline
- **Vogen 8.0+** - Strongly-typed value objects

## Decision

We will refactor the GraphQL schema to adopt:

1. **Nested Namespaces** - Domain-organized query/mutation structure
2. **Relay Node Interface** - Global ID support for all entities
3. **Unified Error Handling** - HotChocolate mutation conventions
4. **Entity-Centric Subscriptions** - `nodeChanged(id: ID!)` pattern
5. **Visibility Directive** - `@visible(to: FAMILY)` for field-level access

### Target Schema Structure

```graphql
type Query {
  auth: AuthQueries!           # login state, token validation
  account: AccountQueries!     # me, settings, profile
  family: FamilyQueries!       # members, invitations
  node(id: ID!): Node          # Relay Node resolution
  nodes(ids: [ID!]!): [Node]!  # Batch Node resolution
}

type Mutation {
  auth: AuthMutations!         # login, logout, register
  account: AccountMutations!   # acceptInvitation
  family: FamilyMutations!     # createFamily, inviteMembers
}

type Subscription {
  nodeChanged(nodeId: ID!): NodeChangedPayload!
  nodeTypeChanged(typeName: String!): NodeChangedPayload!
}
```

### Access Patterns

```graphql
# Namespaced queries
query {
  account {
    myProfile {
      id
      displayName
      birthday @visible(to: FAMILY)
    }
  }
  family {
    current {
      id
      name
      members { id email role }
    }
  }
}

# Namespaced mutations with unified errors
mutation {
  auth {
    login(input: $input) {
      data { accessToken refreshToken }
      errors {
        __typename
        ... on ValidationError { field message }
        ... on BusinessError { code message }
      }
    }
  }
}

# Entity-centric subscription
subscription {
  nodeChanged(nodeId: "VXNlclByb2ZpbGU6MTIz...") {
    nodeId
    changeType
    typeName
    internalId
  }
}
```

## Implementation

### Phase 1-5: Backend Infrastructure (Completed)

**Relay Node Interface:**

```csharp
// INode.cs - Marker interface for Node resolution
public interface INode
{
    Guid Id { get; }
}

// GlobalIdSerializer.cs - Base64 encoding
public static string Serialize(string typeName, Guid id)
    => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{typeName}:{id}"));
```

**Unified Error Hierarchy:**

```csharp
// MutationError.cs - Base class implementing IDefaultMutationError
public abstract record MutationError : IDefaultMutationError
{
    public abstract string Message { get; }
}

public sealed record ValidationError(string Field, string Message) : MutationError;
public sealed record BusinessError(string Code, string Message) : MutationError;
public sealed record NotFoundError(string ResourceType, string Id) : MutationError;
```

**Namespace Container Types:**

```csharp
// Empty marker record for namespace
public sealed record AccountQueries;

// ObjectType configuration
public sealed class AccountQueriesType : ObjectType<AccountQueries>
{
    protected override void Configure(IObjectTypeDescriptor<AccountQueries> descriptor)
    {
        descriptor.Name("AccountQueries");
        descriptor.Description("Account-related queries (profile, settings).");
        descriptor.BindFieldsImplicitly();
    }
}

// Extension adds actual queries
[ExtendObjectType(typeof(AccountQueries))]
public sealed class AccountQueriesExtensions
{
    [Authorize]
    public async Task<UserProfileDto?> MyProfile(
        [Service] IMediator mediator,
        CancellationToken cancellationToken) { ... }
}
```

**Visibility Directive:**

```csharp
// @visible(to: FAMILY) directive
public enum VisibilityLevel { Hidden, Family, Public }

[DirectiveType(DirectiveLocation.FieldDefinition)]
public sealed class VisibleDirective
{
    public VisibilityLevel To { get; }
}
```

### Phase 6: Subscriptions (Completed)

**Entity-Centric Subscriptions:**

```csharp
[ExtendObjectType("Subscription")]
public sealed class NodeSubscriptions
{
    [Subscribe]
    [Topic("node-changed:{nodeId}")]
    [Authorize]
    public async IAsyncEnumerable<NodeChangedPayload> NodeChanged(
        [ID] string nodeId,
        [EventMessage] NodeChangedPayload message,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return message;
    }
}

public sealed record NodeChangedPayload
{
    public required string NodeId { get; init; }
    public required NodeChangeType ChangeType { get; init; }  // Created, Updated, Deleted
    public required string TypeName { get; init; }
    public required Guid InternalId { get; init; }
}
```

**Publishing Changes:**

```csharp
public interface INodeSubscriptionPublisher
{
    Task PublishNodeCreatedAsync<TNode>(Guid internalId, CancellationToken cancellationToken = default);
    Task PublishNodeUpdatedAsync<TNode>(Guid internalId, CancellationToken cancellationToken = default);
    Task PublishNodeDeletedAsync<TNode>(Guid internalId, CancellationToken cancellationToken = default);
}
```

### Phase 7: Frontend Migration (Completed)

**Query Updates:**

```typescript
// Before (flat)
query { myProfile { id displayName } }

// After (namespaced)
query {
  account {
    myProfile { id displayName }
  }
}
```

**Response Type Updates:**

```typescript
// Before
interface GetMyProfileResponse {
  myProfile: UserProfile | null;
}

// After
interface GetMyProfileResponse {
  account: {
    myProfile: UserProfile | null;
  };
}
```

## Rationale

### Why Nested Namespaces?

1. **Discoverability** - Related operations grouped together
2. **Module boundaries** - GraphQL schema reflects DDD modules
3. **Authorization** - Apply `[Authorize]` at namespace level
4. **Evolution** - Easy to add new operations to existing namespaces

### Why Relay Patterns?

1. **Industry standard** - Widely adopted client-side caching patterns
2. **Global IDs** - Unified entity identification across types
3. **Cache normalization** - Apollo Client can cache Node types efficiently
4. **Pagination ready** - Connections pattern for large lists (future)

### Why Entity-Centric Subscriptions?

1. **Simplicity** - Subscribe to any entity by ID
2. **Flexibility** - Client decides which entities to watch
3. **Efficiency** - Only publish to interested subscribers
4. **Security** - Authorization check per subscription

## Alternatives Considered

### Alternative 1: Keep Flat Schema

**Approach:** Leave all queries/mutations at root level.

**Rejected Because:**

- Schema becomes unmanageable with 50+ operations
- Hard to discover related operations
- No clear module boundaries in API

### Alternative 2: Domain-Specific Subscriptions

**Approach:** Create subscriptions like `familyMemberAdded`, `profileUpdated`, etc.

**Rejected Because:**

- Proliferation of subscription types
- Harder to maintain
- Entity-centric pattern more flexible

### Alternative 3: Custom Error Types per Module

**Approach:** Each module defines its own error types.

**Rejected Because:**

- Inconsistent client-side error handling
- Duplication across modules
- HotChocolate mutation conventions solve this better

## Consequences

### Positive

1. **Organized schema** - Domain-aligned query/mutation structure
2. **Unified errors** - Consistent error handling across all mutations
3. **Real-time updates** - Entity subscriptions enable live UIs
4. **Cache-friendly** - Relay patterns enable Apollo cache normalization
5. **Maintainable** - Clear module boundaries in GraphQL layer

### Negative

1. **Migration effort** - Existing clients must update queries
2. **Deeper nesting** - Queries have 2-3 levels instead of 1
3. **Learning curve** - Team must understand namespace pattern

### Migration Path

1. **Backend:** Namespace extensions created alongside legacy root-level operations
2. **Frontend:** Services updated to use namespaced queries
3. **Deprecation:** Legacy operations marked deprecated (future removal)
4. **Cleanup:** Remove legacy operations after frontend migrated

## File Structure

```
src/api/FamilyHub.SharedKernel/Presentation/GraphQL/
├── Relay/
│   ├── INode.cs
│   ├── GlobalIdSerializer.cs
│   └── NodeResolver.cs
├── Errors/
│   ├── MutationError.cs
│   ├── ValidationError.cs
│   ├── BusinessError.cs
│   └── NotFoundError.cs
├── Directives/
│   └── VisibleDirective.cs
└── Subscriptions/
    ├── NodeChangeType.cs
    ├── NodeChangedPayload.cs
    └── INodeSubscriptionPublisher.cs

src/api/FamilyHub.Infrastructure/GraphQL/
├── Subscriptions/
│   ├── NodeSubscriptions.cs
│   └── NodeSubscriptionPublisher.cs
└── Directives/
    └── VisibilityDirectiveRegistration.cs

src/api/FamilyHub.Api/GraphQL/
├── RootQueryExtensions.cs
└── RootMutationExtensions.cs

src/api/Modules/*/Presentation/GraphQL/Namespaces/
├── *Queries.cs          # Empty marker record
├── *QueriesType.cs      # ObjectType configuration
├── *QueriesExtensions.cs # Actual query implementations
├── *Mutations.cs        # Empty marker record
├── *MutationsType.cs    # ObjectType configuration
└── *MutationsExtensions.cs # Actual mutation implementations
```

## Verification

### Schema Verification

```graphql
# Verify namespace structure
query {
  __schema {
    queryType {
      fields { name }  # Should include: auth, account, family, node, nodes
    }
    mutationType {
      fields { name }  # Should include: auth, account, family
    }
    subscriptionType {
      fields { name }  # Should include: nodeChanged, nodeTypeChanged
    }
  }
}
```

### Integration Tests

```csharp
[Fact]
public async Task NamespacedQuery_AccountMyProfile_ReturnsProfile()
{
    var query = """
        query {
          account {
            myProfile { id displayName }
          }
        }
        """;

    var result = await ExecuteGraphQL(query);

    result.Data["account"]["myProfile"]["displayName"].Should().NotBeNull();
}
```

## Related Decisions

- **[ADR-003](ADR-003-GRAPHQL-INPUT-COMMAND-PATTERN.md)** - Input → Command pattern (still applies within namespaced mutations)
- **[ADR-011](ADR-011-DATALOADER-PATTERN.md)** - DataLoader pattern (works with Node resolution)

## References

- [Relay Specification](https://relay.dev/docs/guides/graphql-server-specification/)
- [HotChocolate Mutation Conventions](https://chillicream.com/docs/hotchocolate/v14/defining-a-schema/mutations)
- [Apollo Client Cache Normalization](https://www.apollographql.com/docs/react/caching/cache-configuration/)

## Revision History

| Date | Version | Author | Description |
|------|---------|--------|-------------|
| 2026-01-28 | 1.0 | Andre Kirst | Initial GraphQL schema refactoring |

---

**Decision:** We adopt nested namespaces, Relay patterns, unified error handling, and entity-centric subscriptions for the GraphQL schema. This improves organization, discoverability, and provides a foundation for real-time features while maintaining the Input → Command pattern from ADR-003.
