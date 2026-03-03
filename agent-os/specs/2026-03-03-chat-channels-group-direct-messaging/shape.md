# Chat Channels with Group and Direct Messaging — Shaping Notes

**Feature**: Evolve family-wide messaging into a multi-channel chat system with group and direct messaging
**Created**: 2026-03-03
**GitHub Issue**: #210

---

## Scope

Evolve the existing Messaging module from a single family-wide chat into a full channel-based messaging system. Three channel types are supported:

1. **Family channel** — auto-created when a family is created, all family members are automatically added as participants. Cannot be renamed or have members manually removed.

2. **Group channels** — any family member can create a named group channel and add other family members. Members can be added/removed from group channels.

3. **Direct messages** — 1-on-1 conversations between two family members. Uses a find-or-create pattern to prevent duplicate DM channels.

The frontend gains a Slack-style sidebar + chat panel layout where users can select which conversation to participate in.

## Decisions

- **Channel as a new aggregate root** — channels own their participant collection and have their own lifecycle (create, add/remove members)
- **ChannelId as cross-module VO** — placed in `FamilyHub.Common` so other modules (e.g., event chains) can reference channels
- **Keep FamilyId on Message** — retains data locality for family-level queries, RLS enforcement, and simplifies migration
- **Event-driven automation** — `FamilyCreatedEvent` handler auto-creates the family channel; `UserFamilyAssignedEvent` handler auto-adds new members
- **Two-step migration** — add `channel_id` as nullable, migrate existing data, then make non-nullable
- **Per-channel subscriptions** — subscription topic changes from `MessageSent_{familyId}` to `MessageSent_{channelId}`
- **DM find-or-create** — prevents duplicate DM channels between the same two users in a family
- **Any family member can create groups** — no role restriction on group channel creation
- **Family channel immutable membership** — mirrors family membership, cannot be manually managed

## Context

- **Visuals:** None — following standard chat app patterns (Slack, WhatsApp Web)
- **References:** Existing Messaging module (`src/FamilyHub.Api/Features/Messaging/`)
- **Product alignment:** Phase 1 - Core MVP. Communication is a core family platform capability.

## Standards Applied

- **architecture/ddd-modules** — Channel as new aggregate within Messaging bounded context, feature-folder layout
- **backend/graphql-input-command** — New commands follow Input→Command pattern with dedicated MutationType per command
- **backend/domain-events** — ChannelCreatedEvent, ChannelParticipantAddedEvent, ChannelParticipantRemovedEvent as sealed records
- **backend/vogen-value-objects** — ChannelId, ChannelType, ChannelName, ChannelParticipantId with EfCoreValueConverter
- **backend/user-context** — IUserService for resolving authenticated user in GraphQL resolvers
- **database/ef-core-migrations** — New tables in `messaging` schema with proper indexes
- **frontend/angular-components** — Standalone components with signals-based state
- **frontend/apollo-graphql** — Typed GraphQL operations for channel queries/mutations/subscriptions
- **testing/unit-testing** — xUnit + FluentAssertions with fake repository pattern
