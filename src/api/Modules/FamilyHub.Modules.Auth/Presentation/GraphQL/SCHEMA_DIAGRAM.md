# GraphQL Schema Architecture - Visual Diagram

**Epic:** #24 - Family Member Invitation System
**Version:** 1.0
**Date:** 2026-01-04

---

## Schema Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                      GraphQL API Layer                          │
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐         │
│  │  Mutations   │  │   Queries    │  │ Subscriptions│         │
│  │   (7 ops)    │  │   (4 ops)    │  │   (2 ops)    │         │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘         │
│         │                  │                  │                  │
└─────────┼──────────────────┼──────────────────┼─────────────────┘
          │                  │                  │
          ▼                  ▼                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Input/Command Layer                          │
│                    (ADR-003 Pattern)                            │
│                                                                 │
│  Inputs (primitives)  →  MediatR Commands (Vogen)             │
│                                                                 │
│  InviteFamilyMemberByEmailInput  →  InviteFamilyMemberCommand │
│  CreateManagedMemberInput        →  CreateManagedMemberCommand│
│  BatchInviteFamilyMembersInput   →  BatchInviteCommand        │
│  CancelInvitationInput           →  CancelInvitationCommand   │
│  ResendInvitationInput           →  ResendInvitationCommand   │
│  UpdateInvitationRoleInput       →  UpdateInvitationCommand   │
│  AcceptInvitationInput           →  AcceptInvitationCommand   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Domain Layer                                 │
│                                                                 │
│  FamilyMemberInvitation Aggregate  →  Domain Events            │
│  User Aggregate                    →  Domain Events            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Outbox Pattern                               │
│                                                                 │
│  Domain Events → OutboxMessage → RabbitMQ → Redis PubSub       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────────────────────────────┐
│                    GraphQL Subscriptions                        │
│                                                                 │
│  Redis PubSub → Hot Chocolate → WebSocket → Frontend           │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Mutation Flow

### 1. Email Invitation Flow

```
┌─────────────────┐
│   GraphQL       │
│   Mutation      │
│ inviteFamilyMem │
│ berByEmail      │
└────────┬────────┘
         │
         │  InviteFamilyMemberByEmailInput
         │  { familyId, email, role, message }
         │
         ▼
┌─────────────────┐
│ Mutation Method │
│ (Input → Cmd)   │
└────────┬────────┘
         │
         │  InviteFamilyMemberByEmailCommand
         │  { FamilyId, Email, Role, Message }
         │  (Vogen value objects)
         │
         ▼
┌─────────────────┐
│ Command Handler │
│ (Business Logic)│
└────────┬────────┘
         │
         │  1. Validate email
         │  2. Check duplicates
         │  3. Generate token
         │  4. Create invitation
         │  5. Publish FamilyMemberInvitedEvent
         │
         ▼
┌─────────────────┐
│   Repository    │
│  (Save to DB)   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Outbox Worker  │
│ (Publish Event) │
└────────┬────────┘
         │
         │  FamilyMemberInvitedEvent
         │  → RabbitMQ
         │  → Redis PubSub
         │
         ▼
┌─────────────────┐
│  GraphQL        │
│  Subscription   │
│ (Real-time)     │
└─────────────────┘
         │
         │  PendingInvitationsChangedPayload
         │  { changeType: ADDED, invitation: { ... } }
         │
         ▼
┌─────────────────┐
│    Frontend     │
│   (React/       │
│    Angular)     │
└─────────────────┘
```

### 2. Managed Account Flow

```
┌─────────────────┐
│   GraphQL       │
│   Mutation      │
│ createManaged   │
│    Member       │
└────────┬────────┘
         │
         │  CreateManagedMemberInput
         │  { familyId, username, fullName, role, passwordConfig }
         │
         ▼
┌─────────────────┐
│ Mutation Method │
│ (Input → Cmd)   │
└────────┬────────┘
         │
         │  CreateManagedMemberCommand
         │  { FamilyId, Username, FullName, Role, PasswordConfig }
         │
         ▼
┌─────────────────┐
│ Command Handler │
│ (Business Logic)│
└────────┬────────┘
         │
         │  1. Generate password (based on config)
         │  2. Create Zitadel user
         │  3. Create invitation record
         │  4. Create User entity
         │  5. Publish ManagedAccountCreatedEvent
         │
         ▼
┌─────────────────┐
│  Zitadel API    │
│ (Create User)   │
└────────┬────────┘
         │
         │  ZitadelUserId
         │
         ▼
┌─────────────────┐
│   Repository    │
│  (Save to DB)   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Return Payload │
│ (WITH CREDS!)   │
└─────────────────┘
         │
         │  CreateManagedMemberPayload
         │  {
         │    user: { id, username, fullName },
         │    credentials: {
         │      username,
         │      password,  ← ONLY SHOWN ONCE!
         │      syntheticEmail,
         │      loginUrl
         │    }
         │  }
         │
         ▼
┌─────────────────┐
│    Frontend     │
│ (Display Modal  │
│  with Copy/     │
│  Download)      │
└─────────────────┘
```

---

## Subscription Flow

### Real-time Updates

```
┌─────────────────┐
│    Frontend     │
│  (Subscribe)    │
└────────┬────────┘
         │
         │  subscription {
         │    pendingInvitationsChanged(familyId: "...") {
         │      changeType
         │      invitation { id, email, role }
         │    }
         │  }
         │
         ▼
┌─────────────────┐
│  Hot Chocolate  │
│  Subscription   │
│  Resolver       │
└────────┬────────┘
         │
         │  1. Authenticate user
         │  2. Authorize familyId access
         │  3. Subscribe to Redis channel
         │
         ▼
┌─────────────────┐
│  Redis PubSub   │
│ (Wait for msg)  │
└────────┬────────┘
         │
         │  [Meanwhile... mutation happens]
         │
         │  Domain Event Published:
         │  FamilyMemberInvitedEvent
         │    → OutboxWorker
         │    → RabbitMQ
         │    → Redis PubSub
         │
         ▼
┌─────────────────┐
│  Hot Chocolate  │
│  (Receive msg)  │
└────────┬────────┘
         │
         │  Map event to GraphQL payload
         │
         ▼
┌─────────────────┐
│  WebSocket      │
│  (Push to       │
│   client)       │
└────────┬────────┘
         │
         │  PendingInvitationsChangedPayload
         │  {
         │    familyId: "...",
         │    changeType: ADDED,
         │    invitation: { ... }
         │  }
         │
         ▼
┌─────────────────┐
│    Frontend     │
│  (Update UI)    │
└─────────────────┘
```

---

## Error Flow

### Validation Error

```
┌─────────────────┐
│   GraphQL       │
│   Mutation      │
└────────┬────────┘
         │
         │  Invalid input (e.g., email: "not-an-email")
         │
         ▼
┌─────────────────┐
│ Mutation Method │
│ (Input → Cmd)   │
└────────┬────────┘
         │
         │  Vogen.From() throws ValidationException
         │
         ▼
┌─────────────────┐
│ GraphQL Error   │
│    Filter       │
└────────┬────────┘
         │
         │  Catch exception, map to UserError
         │
         ▼
┌─────────────────┐
│  Return Payload │
│  (with errors)  │
└─────────────────┘
         │
         │  InviteFamilyMemberByEmailPayload {
         │    invitation: null,
         │    errors: [
         │      {
         │        code: INVALID_EMAIL_FORMAT,
         │        message: "Email address 'not-an-email' is invalid.",
         │        field: "email"
         │      }
         │    ],
         │    success: false
         │  }
         │
         ▼
┌─────────────────┐
│    Frontend     │
│ (Display error) │
└─────────────────┘
```

---

## Data Flow: Batch Invitation

### Mixed Mode (Email + Managed)

```
┌─────────────────┐
│   GraphQL       │
│   Mutation      │
│ batchInvite     │
│ FamilyMembers   │
└────────┬────────┘
         │
         │  BatchInviteFamilyMembersInput
         │  {
         │    familyId: "...",
         │    emailInvitations: [
         │      { email: "jane@example.com", role: ADMIN },
         │      { email: "bob@example.com", role: MEMBER }
         │    ],
         │    managedAccounts: [
         │      { username: "emma_smith", fullName: "Emma Smith", role: MANAGED_ACCOUNT, passwordConfig: {...} }
         │    ]
         │  }
         │
         ▼
┌─────────────────┐
│ Command Handler │
│ (Two-Phase)     │
└────────┬────────┘
         │
         │  PHASE 1: Validate All
         │  ├─ Validate all emails
         │  ├─ Validate all usernames
         │  ├─ Check duplicates across both lists
         │  └─ Abort if any errors
         │
         │  PHASE 2: Commit All (Atomic)
         │  ├─ Create email invitations
         │  ├─ Create Zitadel accounts
         │  ├─ Create managed invitations
         │  ├─ Save all to DB
         │  └─ Publish domain events
         │
         ▼
┌─────────────────┐
│  Return Payload │
│ (Batch Results) │
└─────────────────┘
         │
         │  BatchInviteFamilyMembersPayload {
         │    emailInvitations: [
         │      { id, email: "jane@example.com", role: ADMIN },
         │      { id, email: "bob@example.com", role: MEMBER }
         │    ],
         │    managedAccounts: [
         │      {
         │        user: { id, username: "emma_smith" },
         │        credentials: { username, password, syntheticEmail, loginUrl }
         │      }
         │    ],
         │    errors: [],
         │    success: true
         │  }
         │
         ▼
┌─────────────────┐
│    Frontend     │
│ (Display Modal  │
│  with Creds)    │
└─────────────────┘
```

---

## Type Relationships

### Input Types Hierarchy

```
PasswordGenerationConfigInput
  ├─ length: Int (12-32)
  ├─ includeUppercase: Boolean
  ├─ includeLowercase: Boolean
  ├─ includeDigits: Boolean
  └─ includeSymbols: Boolean

InviteFamilyMemberByEmailInput
  ├─ familyId: ID
  ├─ email: String
  ├─ role: UserRole
  └─ message: String?

CreateManagedMemberInput
  ├─ familyId: ID
  ├─ username: String
  ├─ fullName: String
  ├─ role: UserRole
  └─ passwordConfig: PasswordGenerationConfigInput

BatchInviteFamilyMembersInput
  ├─ familyId: ID
  ├─ emailInvitations: [EmailInvitationInput]
  │   ├─ email: String
  │   ├─ role: UserRole
  │   └─ message: String?
  └─ managedAccounts: [ManagedAccountInput]
      ├─ username: String
      ├─ fullName: String
      ├─ role: UserRole
      └─ passwordConfig: PasswordGenerationConfigInput

CancelInvitationInput
  └─ invitationId: ID

ResendInvitationInput
  ├─ invitationId: ID
  └─ message: String?

UpdateInvitationRoleInput
  ├─ invitationId: ID
  └─ newRole: UserRole

AcceptInvitationInput
  └─ token: String
```

### Payload Types Hierarchy

```
PayloadBase (abstract)
  ├─ errors: [UserError]?
  └─ success: Boolean

InviteFamilyMemberByEmailPayload : PayloadBase
  └─ invitation: PendingInvitation?

CreateManagedMemberPayload : PayloadBase
  ├─ invitation: PendingInvitation?
  ├─ user: User?
  └─ credentials: ManagedAccountCredentials?
      ├─ username: String
      ├─ password: String  ← ONLY SHOWN ONCE!
      ├─ syntheticEmail: String
      └─ loginUrl: String

BatchInviteFamilyMembersPayload : PayloadBase
  ├─ emailInvitations: [PendingInvitation]?
  └─ managedAccounts: [ManagedAccountResult]?
      ├─ user: User
      └─ credentials: ManagedAccountCredentials

CancelInvitationPayload : PayloadBase
  └─ success: Boolean

ResendInvitationPayload : PayloadBase
  └─ invitation: PendingInvitation?

UpdateInvitationRolePayload : PayloadBase
  └─ invitation: PendingInvitation?

AcceptInvitationPayload : PayloadBase
  ├─ family: Family?
  └─ role: UserRole?
```

### Subscription Payload Hierarchy

```
ChangeType (enum)
  ├─ ADDED
  ├─ UPDATED
  └─ REMOVED

FamilyMembersChangedPayload
  ├─ familyId: ID
  ├─ changeType: ChangeType
  └─ member: FamilyMemberType?

PendingInvitationsChangedPayload
  ├─ familyId: ID
  ├─ changeType: ChangeType
  └─ invitation: PendingInvitation?
```

---

## Authorization Matrix

| Operation | Required Role | Notes |
|-----------|---------------|-------|
| `inviteFamilyMemberByEmail` | OWNER, ADMIN | Cannot invite as OWNER |
| `createManagedMember` | OWNER, ADMIN | Cannot create as OWNER |
| `batchInviteFamilyMembers` | OWNER, ADMIN | Cannot batch-invite as OWNER |
| `cancelInvitation` | OWNER, ADMIN | Can only cancel own family's invitations |
| `resendInvitation` | OWNER, ADMIN | Can only resend own family's invitations |
| `updateInvitationRole` | OWNER, ADMIN | Cannot update to OWNER |
| `acceptInvitation` | Any authenticated | Email must match invitation |
| `familyMembers` | Any family member | Must be member of the family |
| `pendingInvitations` | OWNER, ADMIN | Only OWNER/ADMIN can see pending |
| `invitation` | OWNER, ADMIN | Only OWNER/ADMIN can query by ID |
| `invitationByToken` | Public | No auth required (for acceptance flow) |
| `familyMembersChanged` | Any family member | Must be member of the family |
| `pendingInvitationsChanged` | OWNER, ADMIN | Only OWNER/ADMIN can subscribe |

---

## Rate Limiting

| Operation | Limit | Window | IP-based |
|-----------|-------|--------|----------|
| `inviteFamilyMemberByEmail` | 10 | 1 hour | Yes |
| `createManagedMember` | 10 | 1 hour | Yes |
| `batchInviteFamilyMembers` | 5 | 1 hour | Yes |
| `acceptInvitation` | 10 | 1 hour | Yes |
| Other mutations | 50 | 1 hour | Yes |
| Queries | Unlimited | - | No |
| Subscriptions | 10 concurrent | - | Per user |

---

## Event Chain Visualization

### Email Invitation → Acceptance

```
1. Admin creates invitation
   ↓
   FamilyMemberInvitedEvent
   ↓
   Email sent (background job)
   ↓
   Redis PubSub → pendingInvitationsChanged (ADDED)

2. Invitee clicks link, accepts
   ↓
   AcceptInvitationCommand
   ↓
   Invitation.Accept()
   ↓
   InvitationAcceptedEvent
   ↓
   User added to family
   ↓
   Redis PubSub → familyMembersChanged (ADDED)
   Redis PubSub → pendingInvitationsChanged (REMOVED)
```

### Managed Account Creation

```
1. Admin creates managed account
   ↓
   CreateManagedMemberCommand
   ↓
   Generate password (based on config)
   ↓
   Zitadel API: Create user
   ↓
   ManagedAccountCreatedEvent
   ↓
   User entity created
   Invitation record created
   ↓
   Return credentials to frontend (ONLY ONCE!)
   ↓
   Redis PubSub → familyMembersChanged (ADDED)
   Redis PubSub → pendingInvitationsChanged (ADDED)
```

---

## Security Considerations

### Credentials Handling

```
┌─────────────────────────────────────────────────────────────┐
│  SECURITY: Managed Account Credentials                     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  1. Password generated in memory (never stored plaintext)  │
│  2. Returned in GraphQL payload ONLY ONCE                  │
│  3. Frontend must display with copy/download options       │
│  4. Never logged, never persisted, never retrievable       │
│  5. Zitadel stores hash (bcrypt/argon2)                    │
│  6. If lost, admin must reset via Zitadel console          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Token Security

```
┌─────────────────────────────────────────────────────────────┐
│  Invitation Token Security                                  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Format: 64 URL-safe base64 characters                     │
│  Entropy: 48 bytes = 384 bits                              │
│  Expiration: 14 days (configurable)                        │
│  Storage: Hashed in database (SHA-256)                     │
│  Rate limiting: 10 attempts per hour per IP                │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

**Diagram Version:** 1.0
**Last Updated:** 2026-01-04
**Agent:** api-designer
