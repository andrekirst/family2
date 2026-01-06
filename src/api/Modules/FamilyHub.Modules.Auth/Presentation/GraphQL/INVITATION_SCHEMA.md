# Family Member Invitation System - GraphQL Schema

**Epic:** #24 - Family Member Invitation System
**Version:** 1.0
**Last Updated:** 2026-01-04
**Status:** Phase 1 - Implementation Ready

---

## Overview

This document defines the complete GraphQL schema for the Family Member Invitation System, including:
- **Mutations:** Create, cancel, resend, accept invitations
- **Queries:** Retrieve family members and pending invitations
- **Subscriptions:** Real-time updates for members and invitations
- **Error Handling:** Unified error codes and validation

---

## Mutations

### 1. inviteFamilyMemberByEmail

Send a token-based email invitation with 14-day expiration.

**Input:**
```graphql
input InviteFamilyMemberByEmailInput {
  familyId: ID!
  email: String!
  role: UserRole!
  message: String  # Optional, max 500 chars
}
```

**Payload:**
```graphql
type InviteFamilyMemberByEmailPayload {
  invitation: PendingInvitation
  errors: [UserError!]
  success: Boolean!
}
```

**Authorization:** Requires OWNER or ADMIN role.

**Example:**
```graphql
mutation {
  inviteFamilyMemberByEmail(input: {
    familyId: "123e4567-e89b-12d3-a456-426614174000"
    email: "jane@example.com"
    role: MEMBER
    message: "Join our family!"
  }) {
    invitation {
      id
      email
      role
      expiresAt
      status
    }
    errors {
      code
      message
      field
    }
  }
}
```

---

### 2. createManagedMember

Create a Zitadel-managed account with auto-generated credentials (for children, elderly, etc.).

**Input:**
```graphql
input CreateManagedMemberInput {
  familyId: ID!
  username: String!
  fullName: String!
  role: UserRole!
  passwordConfig: PasswordGenerationConfigInput!
}

input PasswordGenerationConfigInput {
  length: Int!          # 12-32
  includeUppercase: Boolean!
  includeLowercase: Boolean!
  includeDigits: Boolean!
  includeSymbols: Boolean!
}
```

**Payload:**
```graphql
type CreateManagedMemberPayload {
  invitation: PendingInvitation
  user: User
  credentials: ManagedAccountCredentials  # Returned only once!
  errors: [UserError!]
  success: Boolean!
}

type ManagedAccountCredentials {
  username: String!
  password: String!         # Only shown once - never retrievable again
  syntheticEmail: String!   # {username}@noemail.{env}.family-hub.internal
  loginUrl: String!
}
```

**Authorization:** Requires OWNER or ADMIN role.

**Security:**
- Credentials are returned **only once** during creation
- Never stored in plaintext
- Never retrievable via API after creation
- Frontend must display credentials with copy/download options

**Example:**
```graphql
mutation {
  createManagedMember(input: {
    familyId: "123e4567-e89b-12d3-a456-426614174000"
    username: "emma_smith"
    fullName: "Emma Smith"
    role: MANAGED_ACCOUNT
    passwordConfig: {
      length: 16
      includeUppercase: true
      includeLowercase: true
      includeDigits: true
      includeSymbols: false
    }
  }) {
    user {
      id
      username
      fullName
    }
    credentials {
      username
      password
      syntheticEmail
      loginUrl
    }
    errors {
      code
      message
    }
  }
}
```

---

### 3. batchInviteFamilyMembers

Invite multiple members in a single mutation (mixed mode: email + managed accounts).

**Input:**
```graphql
input BatchInviteFamilyMembersInput {
  familyId: ID!
  emailInvitations: [EmailInvitationInput!]!
  managedAccounts: [ManagedAccountInput!]!
}

input EmailInvitationInput {
  email: String!
  role: UserRole!
  message: String
}

input ManagedAccountInput {
  username: String!
  fullName: String!
  role: UserRole!
  passwordConfig: PasswordGenerationConfigInput!
}
```

**Payload:**
```graphql
type BatchInviteFamilyMembersPayload {
  emailInvitations: [PendingInvitation!]
  managedAccounts: [ManagedAccountResult!]
  errors: [UserError!]
  success: Boolean!
}

type ManagedAccountResult {
  user: User!
  credentials: ManagedAccountCredentials!
}
```

**Authorization:** Requires OWNER or ADMIN role.

**Validation:**
- Two-phase: Validate all inputs, then commit all (atomic)
- Batch size limited to 20 invitations (configurable in appsettings.json)
- Duplicate detection across both email and managed accounts

**Example:**
```graphql
mutation {
  batchInviteFamilyMembers(input: {
    familyId: "123e4567-e89b-12d3-a456-426614174000"
    emailInvitations: [
      { email: "jane@example.com", role: ADMIN }
      { email: "bob@example.com", role: MEMBER }
    ]
    managedAccounts: [
      {
        username: "emma_smith"
        fullName: "Emma Smith"
        role: MANAGED_ACCOUNT
        passwordConfig: { length: 16, includeUppercase: true, includeLowercase: true, includeDigits: true, includeSymbols: false }
      }
    ]
  }) {
    emailInvitations {
      id
      email
      role
    }
    managedAccounts {
      user { id, username }
      credentials { username, password }
    }
    errors {
      code
      message
      field
    }
  }
}
```

---

### 4. cancelInvitation

Cancel a pending invitation (before acceptance).

**Input:**
```graphql
input CancelInvitationInput {
  invitationId: ID!
}
```

**Payload:**
```graphql
type CancelInvitationPayload {
  success: Boolean!
  errors: [UserError!]
}
```

**Authorization:** Requires OWNER or ADMIN role.

**Example:**
```graphql
mutation {
  cancelInvitation(input: {
    invitationId: "123e4567-e89b-12d3-a456-426614174001"
  }) {
    success
    errors {
      code
      message
    }
  }
}
```

---

### 5. resendInvitation

Resend a pending or expired invitation with new token and extended expiration.

**Input:**
```graphql
input ResendInvitationInput {
  invitationId: ID!
  message: String  # Optional updated message
}
```

**Payload:**
```graphql
type ResendInvitationPayload {
  invitation: PendingInvitation
  errors: [UserError!]
  success: Boolean!
}
```

**Authorization:** Requires OWNER or ADMIN role.

**Behavior:**
- Generates new invitation token
- Extends expiration by 14 days
- Updates status to PENDING if previously EXPIRED
- Publishes `FamilyMemberInvitedEvent` with `isResend: true`

**Example:**
```graphql
mutation {
  resendInvitation(input: {
    invitationId: "123e4567-e89b-12d3-a456-426614174001"
    message: "Updated message"
  }) {
    invitation {
      id
      expiresAt
      status
    }
    errors {
      code
      message
    }
  }
}
```

---

### 6. updateInvitationRole

Update the role of a pending invitation (before acceptance).

**Input:**
```graphql
input UpdateInvitationRoleInput {
  invitationId: ID!
  newRole: UserRole!
}
```

**Payload:**
```graphql
type UpdateInvitationRolePayload {
  invitation: PendingInvitation
  errors: [UserError!]
  success: Boolean!
}
```

**Authorization:** Requires OWNER or ADMIN role.

**Validation:**
- Cannot update to OWNER role
- Invitation must be in PENDING status

**Example:**
```graphql
mutation {
  updateInvitationRole(input: {
    invitationId: "123e4567-e89b-12d3-a456-426614174001"
    newRole: ADMIN
  }) {
    invitation {
      id
      role
    }
    errors {
      code
      message
    }
  }
}
```

---

### 7. acceptInvitation

Accept a family invitation using the invitation token.

**Input:**
```graphql
input AcceptInvitationInput {
  token: String!  # 64-character URL-safe base64
}
```

**Payload:**
```graphql
type AcceptInvitationPayload {
  family: Family
  role: UserRole
  errors: [UserError!]
  success: Boolean!
}
```

**Authorization:** Requires authenticated user (any role).

**Validation:**
- Token must be valid and not expired
- User email must match invitation email
- Invitation must be in PENDING status

**Example:**
```graphql
mutation {
  acceptInvitation(input: {
    token: "a7b3c9d2e5f8g1h4i6j7k8l9m0n2o3p4q5r6s7t8u9v0w1x2y3z4a5b6c7d8e9f0"
  }) {
    family {
      id
      name
    }
    role
    errors {
      code
      message
    }
  }
}
```

---

## Queries

### 1. familyMembers

Retrieve all members of a family.

**Signature:**
```graphql
familyMembers(familyId: ID!): [FamilyMemberType!]!
```

**Authorization:** Requires family membership (any role).

**Example:**
```graphql
query {
  familyMembers(familyId: "123e4567-e89b-12d3-a456-426614174000") {
    id
    email
    username
    role
    joinedAt
    isOwner
  }
}
```

---

### 2. pendingInvitations

Retrieve all pending invitations for a family.

**Signature:**
```graphql
pendingInvitations(familyId: ID!): [PendingInvitation!]!
```

**Authorization:** Requires OWNER or ADMIN role.

**Example:**
```graphql
query {
  pendingInvitations(familyId: "123e4567-e89b-12d3-a456-426614174000") {
    id
    email
    username
    role
    status
    invitedAt
    expiresAt
    isExpired
    message
  }
}
```

---

### 3. invitation

Retrieve a single invitation by ID.

**Signature:**
```graphql
invitation(invitationId: ID!): PendingInvitation
```

**Authorization:** Requires OWNER or ADMIN role.

**Example:**
```graphql
query {
  invitation(invitationId: "123e4567-e89b-12d3-a456-426614174001") {
    id
    email
    role
    status
    expiresAt
  }
}
```

---

### 4. invitationByToken

Retrieve an invitation by its token (for acceptance flow).

**Signature:**
```graphql
invitationByToken(token: String!): PendingInvitation
```

**Authorization:** Public (no authentication required).

**Example:**
```graphql
query {
  invitationByToken(token: "a7b3c9d2e5f8g1h4i6j7k8l9m0n2o3p4q5r6s7t8u9v0w1x2y3z4a5b6c7d8e9f0") {
    id
    email
    role
    expiresAt
    isExpired
  }
}
```

---

## Subscriptions

### 1. familyMembersChanged

Real-time updates when family members change.

**Signature:**
```graphql
familyMembersChanged(familyId: ID!): FamilyMembersChangedPayload!
```

**Payload:**
```graphql
type FamilyMembersChangedPayload {
  familyId: ID!
  changeType: ChangeType!  # ADDED, UPDATED, REMOVED
  member: FamilyMemberType
}
```

**Authorization:** Requires family membership (any role).

**Triggers:**
- ADDED: Invitation accepted, new member joined
- UPDATED: Member role changed
- REMOVED: Member left or removed

**Example:**
```graphql
subscription {
  familyMembersChanged(familyId: "123e4567-e89b-12d3-a456-426614174000") {
    changeType
    member {
      id
      email
      role
    }
  }
}
```

---

### 2. pendingInvitationsChanged

Real-time updates when pending invitations change.

**Signature:**
```graphql
pendingInvitationsChanged(familyId: ID!): PendingInvitationsChangedPayload!
```

**Payload:**
```graphql
type PendingInvitationsChangedPayload {
  familyId: ID!
  changeType: ChangeType!  # ADDED, UPDATED, REMOVED
  invitation: PendingInvitation
}
```

**Authorization:** Requires OWNER or ADMIN role.

**Triggers:**
- ADDED: New invitation created
- UPDATED: Invitation resent, role updated
- REMOVED: Invitation accepted, canceled, or expired

**Example:**
```graphql
subscription {
  pendingInvitationsChanged(familyId: "123e4567-e89b-12d3-a456-426614174000") {
    changeType
    invitation {
      id
      email
      role
      status
    }
  }
}
```

---

## Error Handling

### UserError Type

All mutations return a unified error structure:

```graphql
type UserError {
  code: InvitationErrorCode!
  message: String!
  field: String  # Optional: Which input field caused the error
}
```

### Error Codes

```graphql
enum InvitationErrorCode {
  VALIDATION_FAILED
  DUPLICATE_EMAIL
  DUPLICATE_USERNAME
  INVALID_EMAIL_FORMAT
  INVALID_USERNAME_FORMAT
  ZITADEL_API_ERROR
  FAMILY_NOT_FOUND
  UNAUTHORIZED
  RATE_LIMIT_EXCEEDED
  BATCH_SIZE_EXCEEDED
  INVITATION_NOT_FOUND
  INVITATION_EXPIRED
  INVITATION_ALREADY_ACCEPTED
  INVALID_PASSWORD_CONFIG
  INVALID_TOKEN
  FULL_NAME_REQUIRED
  INVALID_ROLE
}
```

### Error Examples

**Validation Error:**
```json
{
  "errors": [
    {
      "code": "INVALID_EMAIL_FORMAT",
      "message": "Email address 'not-an-email' is not a valid email format.",
      "field": "email"
    }
  ]
}
```

**Duplicate Error:**
```json
{
  "errors": [
    {
      "code": "DUPLICATE_EMAIL",
      "message": "Email 'jane@example.com' is already a member or has a pending invitation.",
      "field": "email"
    }
  ]
}
```

**Authorization Error:**
```json
{
  "errors": [
    {
      "code": "UNAUTHORIZED",
      "message": "Only OWNER or ADMIN can invite family members."
    }
  ]
}
```

---

## Redis PubSub Integration

Subscriptions use **Hot Chocolate + Redis PubSub** for multi-instance scaling.

### Configuration

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "FamilyHub:",
    "Channels": {
      "FamilyMembers": "family-members-changed",
      "PendingInvitations": "pending-invitations-changed"
    }
  }
}
```

### Domain Event Publishing

Domain events trigger Redis messages:

1. **InvitationAcceptedEvent** → Publish to `family-members-changed` (ADDED)
2. **FamilyMemberInvitedEvent** → Publish to `pending-invitations-changed` (ADDED)
3. **InvitationCanceledEvent** → Publish to `pending-invitations-changed` (REMOVED)
4. **InvitationUpdatedEvent** → Publish to `pending-invitations-changed` (UPDATED)

---

## Testing Strategy

### GraphQL Schema Tests

```csharp
[Fact]
public async Task Schema_Should_Compile_Without_Errors()
{
    // Verify GraphQL schema builds successfully
    var schema = await new ServiceCollection()
        .AddGraphQL()
        .AddQueryType<Query>()
        .AddMutationType<Mutation>()
        .AddSubscriptionType<Subscription>()
        .BuildSchemaAsync();

    schema.Should().NotBeNull();
}
```

### Mutation Tests (E2E with Playwright)

```typescript
test('should create email invitation', async ({ client }) => {
  const result = await client.mutate(INVITE_BY_EMAIL_MUTATION, {
    input: {
      familyId: familyId,
      email: 'jane@example.com',
      role: 'MEMBER'
    }
  });

  expect(result.data.inviteFamilyMemberByEmail.invitation).toBeDefined();
  expect(result.data.inviteFamilyMemberByEmail.errors).toBeNull();
});
```

### Subscription Tests

```typescript
test('should receive real-time invitation updates', async ({ rabbitmq, client }) => {
  // Subscribe to invitations
  const subscription = client.subscribe(INVITATIONS_CHANGED_SUBSCRIPTION, {
    familyId: familyId
  });

  // Create invitation via mutation
  await client.mutate(INVITE_BY_EMAIL_MUTATION, { ... });

  // Wait for subscription message
  const message = await subscription.next();
  expect(message.value.data.pendingInvitationsChanged.changeType).toBe('ADDED');
});
```

---

## Phase 1 Implementation Checklist

### Workstream B: GraphQL Schema Design (api-designer)

- [x] **1.B.1: Design Invitation Mutations Schema**
  - [x] Create `InviteFamilyMemberByEmailInput`
  - [x] Create `CreateManagedMemberInput`
  - [x] Create `BatchInviteFamilyMembersInput`
  - [x] Create `PasswordGenerationConfigInput`
  - [x] Create `ManagedAccountCredentials` type
  - [x] Create error enum `InvitationErrorCode`
  - [x] Create all mutation payloads

- [x] **1.B.2: Design Query Schema**
  - [x] Create `CancelInvitationInput`
  - [x] Create `ResendInvitationInput`
  - [x] Create `UpdateInvitationRoleInput`
  - [x] Create `AcceptInvitationInput`
  - [x] Document query signatures

- [x] **1.B.3: Design Subscription Schema**
  - [x] Create `ChangeType` enum
  - [x] Create `FamilyMembersChangedPayload`
  - [x] Create `PendingInvitationsChangedPayload`
  - [x] Document Redis PubSub integration

---

## Next Steps

**For backend-developer:**
1. Implement MediatR commands corresponding to each input type
2. Implement mutation resolvers in `InvitationMutations.cs`
3. Implement query resolvers in `InvitationQueries.cs`
4. Implement subscription resolvers in `InvitationSubscriptions.cs`

**For frontend-developer:**
1. Generate TypeScript types from GraphQL schema
2. Create Apollo mutation hooks
3. Create Apollo subscription hooks
4. Implement UI components using generated types

**For test-automator:**
1. Write E2E tests for all mutations
2. Write E2E tests for real-time subscriptions
3. Validate error handling with edge cases

---

**Schema Version:** 1.0
**Compatible with:** Hot Chocolate 14.1.0, .NET 10, Apollo Client 3.x
**Documentation Generated:** 2026-01-04
