# Family-Wide Messaging Channel — Shaping Notes

**Feature**: Family-wide real-time chat channel
**Created**: 2026-02-27
**GitHub Issue**: #203

---

## Scope

A single shared chat channel per family where all members can send and read messages in real-time. This is the initial entry point for the Communication Service module.

**What we're building:**

- Backend: New `Messaging` module with `Message` aggregate, `SendMessage` command, `GetFamilyMessages` query, `MessageSent` GraphQL subscription
- Frontend: New messaging feature with Slack-style feed layout (flat, no bubbles), message input, and real-time updates via WebSocket
- Infrastructure: Enable GraphQL subscriptions (Hot Chocolate + `graphql-ws`)

**What we're NOT building (deferred):**

- 1-on-1 direct messages
- Group sub-channels
- Threading or replies
- Reactions / emoji
- Read receipts
- File attachments or media
- Voice / video messages
- Push notifications for messages
- Message search
- Message editing / deletion

## Decisions

- **Family-wide channel only** — simplest entry point, one channel per family
- **Real-time via GraphQL subscriptions** — uses Hot Chocolate `[Subscribe]` + `[Topic]` with `ITopicEventSender`, frontend uses `graphql-ws` package with Apollo `split()` link
- **In-memory subscriptions** for MVP — `AddInMemorySubscriptions()` (single instance). Upgrade to Redis when scaling.
- **Slack-style UI** — flat message feed with sender avatar, bold name, gray timestamp, content below. No chat bubbles.
- **Cursor pagination** — `before` timestamp parameter for "load older" scrolling. Default 50 messages per page, max 100.
- **Message content** — plain text only, max 4000 characters. Validated via Vogen `MessageContent` value object.
- **Authorization** — family membership enforced by extracting `user.FamilyId` from JWT claims. No fine-grained permissions (all members can send/read). No separate authorization service needed for MVP.
- **Sender info resolved at query time** — `MessageDto` includes `senderName` and `senderAvatarId`, resolved from `IUserRepository` in the query handler. Not denormalized in the database.

## Context

- **Visuals:** Slack-style feed (avatar left, bold sender name + gray timestamp, flat message content below)
- **References:** Family module (`src/FamilyHub.Api/Features/Family/`) — primary reference for module structure, command/query patterns, DDD aggregates, GraphQL namespace types. `ChainSubscriptions.cs` — reference for subscription pattern.
- **Product alignment:** Communication Service is defined as a Generic Subdomain in the domain model (`docs/architecture/domain-model-microservices-map.md`). Direct Messages (RICE: 42.0) and Group Messages (RICE: 38.0) are P1 backlog items. This family-wide channel is the foundation for all future messaging features.

## Standards Applied

- **graphql-input-command** — Input DTOs (primitives) separate from Commands (Vogen types), per-command MutationType
- **permission-system** — Family membership enforcement via user context (no fine-grained permissions for MVP)
- **domain-events** — `MessageSentEvent` raised on aggregate, used for subscription publishing
- **vogen-value-objects** — `MessageId`, `MessageContent` as Vogen VOs with validation
- **user-context** — `IUserService` + `ClaimNames.Sub` for JWT extraction in GraphQL resolvers
- **ef-core-migrations** — `messaging` schema, `Data/MessageConfiguration.cs` with `IEntityTypeConfiguration`
- **rls-policies** — future consideration for messages table (family-level isolation)
- **angular-components** — standalone components with signals, `inject()` DI
- **apollo-graphql** — typed operations with `gql` templates, `provideApollo()` with WebSocket split
- **unit-testing** — xUnit + FluentAssertions, fake repository pattern in TestCommon
