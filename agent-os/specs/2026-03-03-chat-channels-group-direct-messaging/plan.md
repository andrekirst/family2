# Chat Channels with Group and Direct Messaging

**Created**: 2026-03-03
**GitHub Issue**: #210
**Spec**: `agent-os/specs/2026-03-03-chat-channels-group-direct-messaging/`

## Context

The Messaging module currently supports only a single family-wide chat channel. Messages are persisted to PostgreSQL (`messaging.messages` table) and delivered in real-time via GraphQL subscriptions over WebSocket. However, there is no concept of channels — all family members see all messages in one feed.

This feature introduces a **Channel** aggregate that sits between Family and Message, supporting three channel types:

- **Family** — auto-created per family, all members participate (replaces current behavior)
- **Group** — any family member creates, names, and manages membership
- **Direct** — 1-on-1 conversation between two family members (find-or-create pattern)

The frontend gains a **Slack-style sidebar + chat panel** layout where users select which channel to chat in.

## Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| `ChannelId` in `FamilyHub.Common` | Cross-module VO — other modules may reference channels |
| Keep `FamilyId` on `Message` | Data locality, RLS enforcement, simpler migration |
| Channel as `AggregateRoot` | Owns participant collection, has its own lifecycle |
| `ChannelParticipant` as owned entity | Follows `FamilyMember` pattern from Family module |
| Event-driven family channel creation | `FamilyCreatedEvent` handler auto-creates family channel |
| `UserFamilyAssignedEvent` for auto-join | New members auto-join the family channel |
| Two-step nullable migration | Add `channel_id` nullable → migrate data → make non-nullable |
| DM find-or-create | Prevents duplicate DM channels between same two users |
| Per-channel subscription topics | `MessageSent_{channelId}` replaces `MessageSent_{familyId}` |

## Files to Modify

### New Backend Files (~35)

**Domain:**

- `src/FamilyHub.Common/Domain/ValueObjects/ChannelId.cs`
- `src/FamilyHub.Api/Features/Messaging/Domain/ValueObjects/ChannelType.cs`
- `src/FamilyHub.Api/Features/Messaging/Domain/ValueObjects/ChannelName.cs`
- `src/FamilyHub.Api/Features/Messaging/Domain/ValueObjects/ChannelParticipantId.cs`
- `src/FamilyHub.Api/Features/Messaging/Domain/Entities/Channel.cs`
- `src/FamilyHub.Api/Features/Messaging/Domain/Entities/ChannelParticipant.cs`
- `src/FamilyHub.Api/Features/Messaging/Domain/Events/ChannelCreatedEvent.cs`
- `src/FamilyHub.Api/Features/Messaging/Domain/Events/ChannelParticipantAddedEvent.cs`
- `src/FamilyHub.Api/Features/Messaging/Domain/Events/ChannelParticipantRemovedEvent.cs`
- `src/FamilyHub.Api/Features/Messaging/Domain/Repositories/IChannelRepository.cs`

**Infrastructure:**

- `src/FamilyHub.Api/Features/Messaging/Data/ChannelConfiguration.cs`
- `src/FamilyHub.Api/Features/Messaging/Data/ChannelParticipantConfiguration.cs`
- `src/FamilyHub.Api/Features/Messaging/Infrastructure/Repositories/ChannelRepository.cs`

**Application (4 command folders × 5 files + 2 query folders × 3 files + event handlers + models + mapper):**

- `Application/Commands/CreateChannel/` (5 files)
- `Application/Commands/AddChannelMember/` (5 files)
- `Application/Commands/RemoveChannelMember/` (5 files)
- `Application/Commands/CreateOrGetDirectChannel/` (5 files)
- `Application/Queries/GetMyChannels/` (3 files)
- `Application/Queries/GetChannelMessages/` (3 files)
- `Application/EventHandlers/FamilyCreatedEventHandler.cs`
- `Application/EventHandlers/UserFamilyAssignedEventHandler.cs`
- `Application/Mappers/ChannelMapper.cs`
- `Models/ChannelDto.cs`, `ChannelParticipantDto.cs`, `CreateChannelRequest.cs`, etc.

### Modified Backend Files (~12)

- `Message.cs` — add ChannelId
- `MessageSentEvent.cs` — add ChannelId
- `AppDbContext.cs` — add DbSets
- `MessageConfiguration.cs` — add ChannelId mapping
- `IMessageRepository.cs` / `MessageRepository.cs` — add GetByChannelAsync
- `SendMessage/` command files — channel-aware
- `MessagingModule.cs` — register IChannelRepository
- `MessagingSubscriptions.cs` — per-channel topics
- `MessageDto.cs` / `MessageMapper.cs` — add ChannelId

### Frontend Files (~10 new, ~4 modified)

- 6 new components (chat-layout, channel-list, channel-header, create-channel-dialog, dm-initiation, channel-chat)
- Updated messaging.operations.ts, messaging.service.ts, messaging.routes.ts, messaging-page.component.ts

## Implementation Tasks

### Task 1: Save Spec, Commit, and Create GitHub Issue

### Task 2: Backend Domain — Channel Aggregate, Value Objects, Events

### Task 3: Backend Infrastructure — EF Core, Repository, AppDbContext

### Task 4: Database Migration

### Task 5: Backend Event Handlers — Auto-Create Family Channel

### Task 6: Backend Commands — Channel CRUD

### Task 7: Backend — Update SendMessage + New Queries

### Task 8: Backend Unit Tests

### Task 9: Frontend — GraphQL Operations + Service

### Task 10: Frontend — Chat Layout + Channel Components

### Task 11: Frontend — Route Updates

### Task 12: Verification
