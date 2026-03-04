# Relay Cursor Pagination and Scroll UX for Messaging

**Created**: 2026-03-04
**GitHub Issue**: #206
**Spec**: `agent-os/specs/2026-03-04-message-relay-cursor-pagination/`

## Context

The messaging module has basic cursor-based pagination (`before` DateTime + `limit`), but it lacks production hardening. The frontend guesses `hasMore` from result count, there's no composite cursor for duplicate-timestamp safety, and scroll UX is missing features like jump-to-bottom and scroll restoration.

This change adopts Hot Chocolate's Relay connection spec, adds composite cursors, improves scroll UX (Slack/Discord-like), and designs everything to be multi-channel ready for issue #210.

## Files to Modify

### New Files (6)

- `src/FamilyHub.Api/Features/Messaging/Application/Pagination/MessageCursor.cs` -- Composite cursor encode/decode
- `src/FamilyHub.Api/Features/Messaging/Application/Pagination/MessagePage.cs` -- Paged result type
- `src/FamilyHub.Api/Features/Messaging/Application/Queries/GetFamilyMessages/GetFamilyMessagesConnectionQuery.cs` -- Connection query record
- `src/FamilyHub.Api/Features/Messaging/Application/Queries/GetFamilyMessages/GetFamilyMessagesConnectionQueryHandler.cs` -- Connection handler
- `tests/FamilyHub.Messaging.Tests/Features/Messaging/Application/MessageCursorTests.cs` -- Cursor unit tests
- `tests/FamilyHub.Messaging.Tests/Features/Messaging/Application/GetFamilyMessagesConnectionQueryHandlerTests.cs` -- Handler tests

### Modified Files (9)

- `src/FamilyHub.Api/Features/Messaging/Domain/Repositories/IMessageRepository.cs` -- Add paged method
- `src/FamilyHub.Api/Features/Messaging/Infrastructure/Repositories/MessageRepository.cs` -- Implement paged method
- `src/FamilyHub.Api/Features/Messaging/Data/MessageConfiguration.cs` -- Update composite index
- `src/FamilyHub.Api/Features/Messaging/Application/Queries/GetFamilyMessages/QueryType.cs` -- Relay connection return
- `src/frontend/family-hub-web/src/app/features/messaging/graphql/messaging.operations.ts` -- Connection query
- `src/frontend/family-hub-web/src/app/features/messaging/services/messaging.service.ts` -- Connection types + method
- `src/frontend/family-hub-web/src/app/features/messaging/components/messaging-page/messaging-page.component.ts` -- Cursor-based paging
- `src/frontend/family-hub-web/src/app/features/messaging/components/message-list/message-list.component.ts` -- Scroll restoration, FAB, separator
- `tests/FamilyHub.TestCommon/Fakes/FakeMessageRepository.cs` -- Add paged method

## Implementation Tasks

### Task 1: Save Spec, Update Issue #206, and Commit

1. Write spec files to `agent-os/specs/2026-03-04-message-relay-cursor-pagination/`
2. Update GitHub issue #206 with proper title, labels, and body
3. Git commit spec files

### Task 2: Backend Pagination Infrastructure

#### 2.1: Create `MessageCursor` utility

**New:** `src/FamilyHub.Api/Features/Messaging/Application/Pagination/MessageCursor.cs`

- Encode: `Base64("{SentAt.Ticks}|{MessageId}")`
- Decode + TryDecode for safe parsing
- Pure static utility, no dependencies

#### 2.2: Create `MessagePage<T>` result type

**New:** `src/FamilyHub.Api/Features/Messaging/Application/Pagination/MessagePage.cs`

- Record: `(List<T> Items, bool HasNextPage, bool HasPreviousPage, string? StartCursor, string? EndCursor)`

#### 2.3: Extend repository interface

**Modify:** `src/FamilyHub.Api/Features/Messaging/Domain/Repositories/IMessageRepository.cs`

- Add: `GetPagedByFamilyAsync(FamilyId, int first, string? afterCursor, CancellationToken)`
- Returns `(List<Message>, bool HasNextPage)` -- cursor encoding stays in application layer

#### 2.4: Implement paged repository method

**Modify:** `src/FamilyHub.Api/Features/Messaging/Infrastructure/Repositories/MessageRepository.cs`

- Composite cursor WHERE: `(SentAt < cursor) OR (SentAt == cursor AND Id < cursorId)`
- N+1 fetch pattern: request `first + 1`, trim if overflow -> `hasNextPage = true`
- Order: `SentAt DESC, Id DESC`
- Cap at 100

#### 2.5: Update database index

**Modify:** `src/FamilyHub.Api/Features/Messaging/Data/MessageConfiguration.cs`

- Change from `(FamilyId, SentAt DESC)` -> `(FamilyId, SentAt DESC, Id DESC)`
- Generate EF Core migration

#### 2.6: Create connection query + handler

**New:** `GetFamilyMessagesConnectionQuery` record + handler

- Calls paged repository, resolves sender info, encodes cursors via `MessageCursor`
- Returns `MessagePage<MessageDto>`

#### 2.7: Update GraphQL resolver to Relay connection

**Modify:** `src/FamilyHub.Api/Features/Messaging/Application/Queries/GetFamilyMessages/QueryType.cs`

- Use `[UsePaging]` attribute with `Connection<MessageDto>` return
- Schema: `messages(first, after)` -> `MessagingMessagesConnection { edges { cursor, node }, pageInfo }`

### Task 3: Frontend Pagination Update

#### 3.1: Update GraphQL operations

- Replace flat list query with Relay connection query (edges/nodes/pageInfo)

#### 3.2: Update `MessagingService`

- Add `PageInfo`, `MessageEdge`, `MessageConnection` interfaces
- Return `MessagePageResult { messages, pageInfo }` instead of `MessageDto[]`

#### 3.3: Update `MessagingPageComponent`

- Replace count-guessing with real `pageInfo.hasNextPage`
- Store `endCursor` signal, pass to `loadOlderMessages()`
- Track `oldestCursor` across paginated loads

#### 3.4: Enhance `MessageListComponent` scroll UX

- **Scroll restoration:** Save `scrollHeight` before prepend, restore `scrollTop = newHeight - oldHeight`
- **Jump-to-bottom FAB:** Show when scrolled up, with unread count badge
- **Unread separator:** Visual line before first message received via subscription while scrolled up
- **Debounce:** Prevent multiple `loadOlder` emissions at scroll top

### Task 4: Tests

#### 4.1: `MessageCursor` unit tests

- Roundtrip encode/decode, invalid input handling, duplicate-timestamp safety

#### 4.2: Update `FakeMessageRepository`

- Add `GetPagedByFamilyAsync` with composite cursor logic + N+1 pattern

#### 4.3: Connection query handler tests

- Initial load, cursor-based paging, duplicate-timestamp edge case, empty results, cursor/pageInfo correctness

### Task 5: Database Migration

Generate migration: `dotnet ef migrations add UpdateMessageIndexForCompositeCursor`

## Channel-Agnostic Design

When #210 (chat channels) lands, pagination extends naturally:

| Component | Current (FamilyId) | Future (ChannelId) |
|-----------|-------------------|-------------------|
| `MessageCursor` | No change | No change |
| `MessagePage<T>` | No change | No change |
| Repository | `GetPagedByFamilyAsync` | Add `GetPagedByChannelAsync` |
| DB index | `(FamilyId, SentAt, Id)` | Add `(ChannelId, SentAt, Id)` |
| GraphQL | `messages(first, after)` | Add `channelMessages(channelId, first, after)` |

## Risks

| Risk | Mitigation |
|------|-----------|
| Vogen `MessageId` comparison may not translate in EF Core | Test early; fallback to `.Value` property |
| `[UsePaging]` + manual `Connection<T>` conflict | HC15 docs confirm this pattern; verify with schema introspection |
| Breaking GraphQL schema change | Deploy frontend + backend atomically; no external consumers |
| Scroll restoration flicker | Use `requestAnimationFrame` for batching |

## Verification

1. **Backend:** `dotnet test` -- all existing + new tests pass
2. **Schema:** Introspect GraphQL schema, verify `MessagingMessagesConnection` type
3. **Frontend:** Load messaging page, verify initial load with real `hasNextPage`
4. **Pagination:** Scroll to top, verify older messages load with cursor
5. **Duplicate timestamps:** Insert 2 messages with same `SentAt`, verify both paginate correctly
6. **Scroll UX:** Load older -> scroll position stays stable (no jump)
7. **Jump-to-bottom:** Scroll up, receive subscription message, verify FAB with unread badge
8. **Migration:** Verify composite index in PostgreSQL
