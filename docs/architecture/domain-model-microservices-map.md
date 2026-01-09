# Domain Model & Microservices Architecture

## Family Hub - Domain-Driven Design Analysis

**Document Version:** 1.1
**Date:** 2026-01-09 (Updated)
**Status:** Living Document
**Author:** Business Analyst (Claude Code)
**Related ADRs:** [ADR-001 (Modular Monolith First)](ADR-001-MODULAR-MONOLITH-FIRST.md), [ADR-005 (Family Module Extraction Pattern)](ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md)

---

## Executive Summary

This document defines the Domain-Driven Design (DDD) bounded contexts, domain models, and microservices architecture for the Family Hub application. The architecture is designed for incremental implementation by a single developer with AI assistance, emphasizing event-driven automation and clear service boundaries.

**Key Differentiator:** Event chains enable automated cross-domain workflows (e.g., doctor appointment → calendar → shopping list → task reminder).

**Current Implementation Status (Phase 0-1):**

- **Architecture:** Modular Monolith (see [ADR-001](ADR-001-MODULAR-MONOLITH-FIRST.md))
- **Family Module:** Extracted as separate bounded context (see [ADR-005](ADR-005-FAMILY-MODULE-EXTRACTION-PATTERN.md))
- **Microservices Migration:** Planned for Phase 5+ using Strangler Fig pattern

---

## 1. Bounded Contexts Overview

### 1.1 Context Map

```
┌─────────────────────────────────────────────────────────────────┐
│                        Family Hub Platform                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │   Auth       │────────▶│  Identity    │                      │
│  │   Service    │         │  Context     │                      │
│  │  (Zitadel)   │         │   (Users)    │                      │
│  └──────────────┘         └──────────────┘                      │
│         │                        │                              │
│         │                        ▼                              │
│         │              ┌─────────────────┐                      │
│         │              │  Communication  │                      │
│         │              │    Service      │                      │
│         │              └─────────────────┘                      │
│         │                        │                              │
│         ▼                        ▼                              │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │   Calendar   │◀───────▶│    Task      │                      │
│  │   Service    │         │   Service    │                      │
│  └──────────────┘         └──────────────┘                      │
│         │                        │                              │
│         ▼                        ▼                              │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │   Health     │────────▶│  Shopping    │                      │
│  │   Service    │         │   Service    │                      │
│  └──────────────┘         └──────────────┘                      │
│         │                        │                              │
│         ▼                        ▼                              │
│  ┌──────────────┐         ┌──────────────┐                      │
│  │ Meal Planning│◀───────▶│   Finance    │                      │
│  │   Service    │         │   Service    │                      │
│  └──────────────┘         └──────────────┘                      │
│                                                                 │
│  ┌─────────────────────────────────────────┐                    │
│  │         Event Bus (Redis Pub/Sub)        │                   │
│  └─────────────────────────────────────────┘                    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

Legend:
────▶  Synchronous API calls
◀───▶  Bi-directional event flow
```

### 1.2 Bounded Context Definitions

| Context                   | Core Responsibility                                  | Team Pattern      |
| ------------------------- | ---------------------------------------------------- | ----------------- |
| **Auth Service**          | Identity & access management via Zitadel             | Conformist        |
| **Federation Service**    | Instance federation, cross-instance communication    | **Core Domain**   |
| **Calendar Service**      | Schedule management, events, appointments            | Core Domain       |
| **Task Service**          | To-do items, reminders, task tracking                | Core Domain       |
| **Shopping Service**      | Shopping lists, item management                      | Supporting Domain |
| **Meal Planning Service** | Meal plans, recipes, nutrition                       | Supporting Domain |
| **Health Service**        | Medical appointments, prescriptions, health tracking | Supporting Domain |
| **Finance Service**       | Budget tracking, expenses, family finances           | Supporting Domain |
| **Communication Service** | Notifications, messaging, alerts                     | Generic Subdomain |

### 1.3 Fediverse Architecture

**Family Hub implements a federated architecture** inspired by ActivityPub and Mastodon, allowing:

- **Self-hosted instances** keep data locally (full privacy and control)
- **Cloud-hosted instances** available as managed service on any Kubernetes provider
- **Instance federation** enables cross-instance family connections
- **Cloud-agnostic deployment** via Kubernetes (any hosting provider, not just Azure/AWS/GCP)

**Example Use Cases:**

- Grandparents self-host their instance while parents use cloud-hosted service - both can connect and share calendars
- Extended family members on different instances can form federated family groups
- Data sovereignty: each instance owner controls their data while still connecting to the network

---

## 2. Detailed Bounded Context Specifications

### 2.1 Auth Service (Identity & Access Management)

**Type:** External Integration (Zitadel)
**Pattern:** Anti-Corruption Layer

#### Core Responsibilities

- User authentication and authorization
- Family group management
- Permission and role management
- Token validation and refresh
- Session management

#### Core Entities & Aggregates

```csharp
// Aggregate Root
public class FamilyGroup
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Guid OwnerId { get; private set; }
    public List<FamilyMember> Members { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Domain methods
    public void AddMember(Guid userId, FamilyRole role);
    public void RemoveMember(Guid userId);
    public void UpdateMemberRole(Guid userId, FamilyRole newRole);
}

// Entity
public class FamilyMember
{
    public Guid UserId { get; private set; }
    public FamilyRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public bool IsActive { get; private set; }
}

// Aggregate Root
public class FamilyMemberInvitation
{
    public Guid InvitationId { get; private set; }
    public Guid FamilyId { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public InvitationStatus Status { get; private set; }
    public FamilyRole Role { get; private set; }
    public InvitationType Type { get; private set; }

    // Email invitation fields
    public string? InviteeEmail { get; private set; }
    public string? Token { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    // Child account creation fields
    public string? ChildUsername { get; private set; }
    public string? ChildFullName { get; private set; }
    public string? ZitadelUserId { get; private set; }

    // Domain methods
    public void Accept(Guid acceptedByUserId);
    public void Decline(string? reason);
    public bool IsExpired();
}

// Value Objects
public enum FamilyRole
{
    Owner,
    Admin,
    Member,
    Child
}

public enum InvitationStatus
{
    Pending,
    Accepted,
    Declined,
    Expired,
    Cancelled,
    ChildAccountCreated
}

public enum InvitationType
{
    EmailInvitation,
    ChildAccount
}
```

#### Domain Events Published

```csharp
public record UserRegisteredEvent(Guid UserId, string Email, DateTime RegisteredAt);
public record FamilyGroupCreatedEvent(Guid GroupId, Guid OwnerId, string Name);
public record MemberAddedToFamilyEvent(Guid GroupId, Guid UserId, FamilyRole Role);
public record MemberRemovedFromFamilyEvent(Guid GroupId, Guid UserId);
public record UserAuthenticatedEvent(Guid UserId, DateTime AuthenticatedAt);

// Family Member Invitation Events
public record FamilyMemberInvitedEvent(
    Guid InvitationId,
    Guid FamilyId,
    string InviteeEmail,
    FamilyRole Role,
    Guid InvitedByUserId,
    string Token,
    DateTime ExpiresAt
);

public record ChildAccountCreatedEvent(
    Guid InvitationId,
    Guid FamilyId,
    Guid ChildUserId,
    string Username,
    string FullName,
    FamilyRole Role,
    Guid CreatedByUserId,
    string ZitadelUserId
);

public record FamilyMemberInvitationAcceptedEvent(
    Guid InvitationId,
    Guid FamilyId,
    Guid AcceptedByUserId,
    FamilyRole Role
);
```

#### Domain Events Consumed

- None (root context for authentication)

#### GraphQL API Schema Outline

```graphql
type User {
  id: ID!
  email: String!
  displayName: String
  familyGroups: [FamilyGroup!]!
}

type FamilyGroup {
  id: ID!
  name: String!
  owner: User!
  members: [FamilyMember!]!
  createdAt: DateTime!
}

type FamilyMember {
  user: User!
  role: FamilyRole!
  joinedAt: DateTime!
  isActive: Boolean!
}

enum FamilyRole {
  OWNER
  ADMIN
  MEMBER
  CHILD
}

type PendingInvitation {
  invitationId: ID!
  inviteeEmail: String
  childUsername: String
  role: FamilyRole!
  status: InvitationStatus!
  invitedByUserName: String!
  createdAt: DateTime!
  expiresAt: DateTime
}

enum InvitationStatus {
  PENDING
  ACCEPTED
  DECLINED
  EXPIRED
  CANCELLED
  CHILD_ACCOUNT_CREATED
}

type Query {
  me: User!
  myFamilies: [FamilyGroup!]!
  familyGroup(id: ID!): FamilyGroup
  pendingInvitations: [PendingInvitation!]!
}

type Mutation {
  createFamilyGroup(name: String!): FamilyGroup!

  # Invitation mutations
  inviteFamilyMemberByEmail(input: InviteFamilyMemberByEmailInput!): InviteFamilyMemberByEmailPayload!
  createChildMember(input: CreateChildMemberInput!): CreateChildMemberPayload!
  batchInviteFamilyMembers(input: BatchInviteFamilyMembersInput!): BatchInviteFamilyMembersPayload!
  acceptInvitation(input: AcceptInvitationInput!): AcceptInvitationPayload!

  # Legacy mutations
  inviteFamilyMember(
    groupId: ID!
    email: String!
    role: FamilyRole!
  ): FamilyMember!
  removeFamilyMember(groupId: ID!, userId: ID!): Boolean!
  updateMemberRole(groupId: ID!, userId: ID!, role: FamilyRole!): FamilyMember!
}
```

#### Storage Strategy

- **Primary Database:** PostgreSQL
- **Schema:** `auth` schema
- **Tables:** `family_groups`, `family_members`, `user_profiles`
- **Caching:** Redis for session tokens and user context
- **External:** Zitadel for OAuth tokens and user credentials

---

### 2.2 Calendar Service

**Type:** Core Domain
**Pattern:** Event Sourcing for audit trail

#### Core Responsibilities

- Event and appointment scheduling
- Calendar sharing within family
- Recurring event management
- Event notifications and reminders
- Integration with external calendars (future)

#### Core Entities & Aggregates

```csharp
// Aggregate Root
public class CalendarEvent
{
    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public Guid CreatedBy { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public EventLocation Location { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public bool IsAllDay { get; private set; }
    public EventType Type { get; private set; }
    public RecurrencePattern Recurrence { get; private set; }
    public List<Guid> Attendees { get; private set; }
    public EventMetadata Metadata { get; private set; }

    // Domain methods
    public void Reschedule(DateTime newStart, DateTime newEnd);
    public void Cancel(string reason);
    public void AddAttendee(Guid userId);
    public void UpdateDetails(string title, string description, EventLocation location);
}

// Value Objects
public record EventLocation(string Address, decimal? Latitude, decimal? Longitude);

public enum EventType
{
    Personal,
    Medical,
    School,
    Work,
    Social,
    Travel,
    Other
}

public record RecurrencePattern(
    RecurrenceFrequency Frequency,
    int Interval,
    List<DayOfWeek> DaysOfWeek,
    DateTime? EndDate,
    int? Occurrences
);

public enum RecurrenceFrequency
{
    None,
    Daily,
    Weekly,
    Monthly,
    Yearly
}

// Entity for metadata extensibility
public class EventMetadata
{
    public string RelatedEntityType { get; set; } // e.g., "HealthAppointment"
    public Guid? RelatedEntityId { get; set; }
    public Dictionary<string, string> CustomFields { get; set; }
}
```

#### Domain Events Published

```csharp
public record CalendarEventCreatedEvent(
    Guid EventId,
    Guid FamilyGroupId,
    string Title,
    DateTime StartTime,
    DateTime EndTime,
    EventType Type,
    Guid CreatedBy
);

public record CalendarEventUpdatedEvent(
    Guid EventId,
    string Title,
    DateTime StartTime,
    DateTime EndTime
);

public record CalendarEventCancelledEvent(
    Guid EventId,
    string Reason,
    DateTime CancelledAt
);

public record CalendarEventRescheduledEvent(
    Guid EventId,
    DateTime OldStartTime,
    DateTime OldEndTime,
    DateTime NewStartTime,
    DateTime NewEndTime
);

public record CalendarEventReminderDueEvent(
    Guid EventId,
    Guid UserId,
    DateTime ReminderTime
);
```

#### Domain Events Consumed

```csharp
// From Health Service
public record HealthAppointmentScheduledEvent(
    Guid AppointmentId,
    string PatientName,
    string DoctorName,
    DateTime AppointmentTime,
    string Location
);

// From Task Service
public record TaskDeadlineApproachingEvent(
    Guid TaskId,
    string TaskTitle,
    DateTime Deadline
);
```

#### GraphQL API Schema Outline

```graphql
type CalendarEvent {
  id: ID!
  familyGroup: FamilyGroup!
  title: String!
  description: String
  location: EventLocation
  startTime: DateTime!
  endTime: DateTime!
  isAllDay: Boolean!
  type: EventType!
  recurrence: RecurrencePattern
  attendees: [User!]!
  createdBy: User!
  metadata: EventMetadata
}

type EventLocation {
  address: String!
  latitude: Float
  longitude: Float
}

type RecurrencePattern {
  frequency: RecurrenceFrequency!
  interval: Int!
  daysOfWeek: [DayOfWeek!]
  endDate: DateTime
  occurrences: Int
}

enum EventType {
  PERSONAL
  MEDICAL
  SCHOOL
  WORK
  SOCIAL
  TRAVEL
  OTHER
}

type Query {
  calendarEvents(
    familyGroupId: ID!
    startDate: DateTime!
    endDate: DateTime!
    type: EventType
  ): [CalendarEvent!]!

  calendarEvent(id: ID!): CalendarEvent

  upcomingEvents(familyGroupId: ID!, days: Int = 7): [CalendarEvent!]!
}

type Mutation {
  createCalendarEvent(input: CreateCalendarEventInput!): CalendarEvent!
  updateCalendarEvent(id: ID!, input: UpdateCalendarEventInput!): CalendarEvent!
  cancelCalendarEvent(id: ID!, reason: String): Boolean!
  rescheduleEvent(
    id: ID!
    newStartTime: DateTime!
    newEndTime: DateTime!
  ): CalendarEvent!
}

type Subscription {
  calendarEventUpdated(familyGroupId: ID!): CalendarEvent!
}
```

#### Storage Strategy

- **Primary Database:** PostgreSQL
- **Schema:** `calendar` schema
- **Tables:** `calendar_events`, `event_recurrences`, `event_attendees`, `event_metadata`
- **Event Store:** Separate `calendar_events_history` for event sourcing
- **Caching:** Redis for upcoming events (7-day window)
- **Indexing:** Composite indexes on `(family_group_id, start_time)`, `(created_by, start_time)`

---

### 2.3 Task Service

**Type:** Core Domain
**Pattern:** Traditional DDD with event publishing

#### Core Responsibilities

- Task and to-do management
- Task assignment and tracking
- Recurring tasks
- Task prioritization and categorization
- Deadline reminders

#### Core Entities & Aggregates

```csharp
// Aggregate Root
public class Task
{
    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public Guid? AssignedTo { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public RecurrencePattern Recurrence { get; private set; }
    public TaskCategory Category { get; private set; }
    public List<SubTask> SubTasks { get; private set; }
    public TaskMetadata Metadata { get; private set; }

    // Domain methods
    public void AssignTo(Guid userId);
    public void Complete();
    public void Reopen();
    public void UpdatePriority(TaskPriority priority);
    public void SetDueDate(DateTime dueDate);
    public void AddSubTask(string title);
}

// Value Objects
public enum TaskStatus
{
    NotStarted,
    InProgress,
    Completed,
    Cancelled
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum TaskCategory
{
    Personal,
    Household,
    Shopping,
    Medical,
    Financial,
    School,
    Work,
    Other
}

// Entity
public class SubTask
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public bool IsCompleted { get; private set; }
    public int Order { get; private set; }
}

public class TaskMetadata
{
    public string RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public Dictionary<string, string> CustomFields { get; set; }
}
```

#### Domain Events Published

```csharp
public record TaskCreatedEvent(
    Guid TaskId,
    Guid FamilyGroupId,
    string Title,
    TaskPriority Priority,
    DateTime? DueDate,
    Guid CreatedBy
);

public record TaskAssignedEvent(
    Guid TaskId,
    Guid AssignedTo,
    Guid AssignedBy
);

public record TaskCompletedEvent(
    Guid TaskId,
    Guid CompletedBy,
    DateTime CompletedAt
);

public record TaskDueDateApproachingEvent(
    Guid TaskId,
    Guid? AssignedTo,
    DateTime DueDate,
    int HoursUntilDue
);

public record TaskOverdueEvent(
    Guid TaskId,
    Guid? AssignedTo,
    DateTime DueDate,
    int HoursOverdue
);
```

#### Domain Events Consumed

```csharp
// From Calendar Service
public record CalendarEventCreatedEvent(
    Guid EventId,
    string Title,
    DateTime StartTime
);

// From Shopping Service
public record ShoppingListCreatedEvent(
    Guid ListId,
    string Name,
    Guid CreatedBy
);

// From Health Service
public record PrescriptionFilledReminderEvent(
    Guid PrescriptionId,
    string MedicationName,
    DateTime RefillDate
);
```

#### GraphQL API Schema Outline

```graphql
type Task {
  id: ID!
  familyGroup: FamilyGroup!
  title: String!
  description: String
  status: TaskStatus!
  priority: TaskPriority!
  assignedTo: User
  createdBy: User!
  dueDate: DateTime
  createdAt: DateTime!
  completedAt: DateTime
  recurrence: RecurrencePattern
  category: TaskCategory!
  subTasks: [SubTask!]!
  metadata: TaskMetadata
}

type SubTask {
  id: ID!
  title: String!
  isCompleted: Boolean!
  order: Int!
}

enum TaskStatus {
  NOT_STARTED
  IN_PROGRESS
  COMPLETED
  CANCELLED
}

enum TaskPriority {
  LOW
  MEDIUM
  HIGH
  URGENT
}

enum TaskCategory {
  PERSONAL
  HOUSEHOLD
  SHOPPING
  MEDICAL
  FINANCIAL
  SCHOOL
  WORK
  OTHER
}

type Query {
  tasks(
    familyGroupId: ID!
    status: TaskStatus
    assignedTo: ID
    category: TaskCategory
  ): [Task!]!

  task(id: ID!): Task

  myTasks(status: TaskStatus): [Task!]!

  overdueTasks(familyGroupId: ID!): [Task!]!
}

type Mutation {
  createTask(input: CreateTaskInput!): Task!
  updateTask(id: ID!, input: UpdateTaskInput!): Task!
  assignTask(id: ID!, userId: ID!): Task!
  completeTask(id: ID!): Task!
  reopenTask(id: ID!): Task!
  deleteTask(id: ID!): Boolean!
}

type Subscription {
  taskUpdated(familyGroupId: ID!): Task!
}
```

#### Storage Strategy

- **Primary Database:** PostgreSQL
- **Schema:** `tasks` schema
- **Tables:** `tasks`, `sub_tasks`, `task_metadata`
- **Caching:** Redis for active tasks per user
- **Indexing:** Composite indexes on `(family_group_id, status, due_date)`, `(assigned_to, status)`

---

### 2.4 Shopping Service

**Type:** Supporting Domain
**Pattern:** Simple CRUD with event publishing

#### Core Responsibilities

- Shopping list management
- Item tracking and categorization
- Shared family shopping lists
- Purchase history
- Recipe-to-shopping-list conversion

#### Core Entities & Aggregates

```csharp
// Aggregate Root
public class ShoppingList
{
    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public string Name { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ShoppingListStatus Status { get; private set; }
    public List<ShoppingItem> Items { get; private set; }

    // Domain methods
    public void AddItem(string name, string category, int quantity, string unit);
    public void RemoveItem(Guid itemId);
    public void MarkItemAsPurchased(Guid itemId);
    public void CompleteList();
}

// Entity
public class ShoppingItem
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Category { get; private set; }
    public int Quantity { get; private set; }
    public string Unit { get; private set; }
    public bool IsPurchased { get; private set; }
    public DateTime? PurchasedAt { get; private set; }
    public decimal? EstimatedPrice { get; private set; }
    public string Notes { get; private set; }
}

// Value Objects
public enum ShoppingListStatus
{
    Active,
    InProgress,
    Completed,
    Archived
}
```

#### Domain Events Published

```csharp
public record ShoppingListCreatedEvent(
    Guid ListId,
    Guid FamilyGroupId,
    string Name,
    Guid CreatedBy
);

public record ShoppingItemAddedEvent(
    Guid ListId,
    Guid ItemId,
    string ItemName,
    string Category,
    int Quantity
);

public record ShoppingListCompletedEvent(
    Guid ListId,
    DateTime CompletedAt,
    int ItemCount
);

public record PrescriptionAddedToShoppingListEvent(
    Guid ListId,
    Guid PrescriptionId,
    string MedicationName
);
```

#### Domain Events Consumed

```csharp
// From Health Service
public record PrescriptionIssuedEvent(
    Guid PrescriptionId,
    string MedicationName,
    string Dosage,
    DateTime IssuedDate
);

// From Meal Planning Service
public record MealPlannedEvent(
    Guid MealPlanId,
    string MealName,
    List<Ingredient> Ingredients
);
```

#### GraphQL API Schema Outline

```graphql
type ShoppingList {
  id: ID!
  familyGroup: FamilyGroup!
  name: String!
  createdBy: User!
  createdAt: DateTime!
  status: ShoppingListStatus!
  items: [ShoppingItem!]!
}

type ShoppingItem {
  id: ID!
  name: String!
  category: String
  quantity: Int!
  unit: String
  isPurchased: Boolean!
  purchasedAt: DateTime
  estimatedPrice: Decimal
  notes: String
}

enum ShoppingListStatus {
  ACTIVE
  IN_PROGRESS
  COMPLETED
  ARCHIVED
}

type Query {
  shoppingLists(
    familyGroupId: ID!
    status: ShoppingListStatus
  ): [ShoppingList!]!

  shoppingList(id: ID!): ShoppingList

  activeShoppingLists(familyGroupId: ID!): [ShoppingList!]!
}

type Mutation {
  createShoppingList(input: CreateShoppingListInput!): ShoppingList!
  addItemToList(listId: ID!, input: AddShoppingItemInput!): ShoppingItem!
  markItemPurchased(listId: ID!, itemId: ID!): ShoppingItem!
  removeItemFromList(listId: ID!, itemId: ID!): Boolean!
  completeShoppingList(id: ID!): ShoppingList!
}
```

#### Storage Strategy

- **Primary Database:** PostgreSQL
- **Schema:** `shopping` schema
- **Tables:** `shopping_lists`, `shopping_items`
- **Caching:** Redis for active shopping lists
- **Indexing:** Indexes on `(family_group_id, status)`, `(created_by, created_at)`

---

### 2.5 Meal Planning Service

**Type:** Supporting Domain
**Pattern:** Simple DDD

#### Core Responsibilities

- Meal planning and scheduling
- Recipe management
- Nutritional information
- Ingredient-to-shopping-list conversion
- Family meal preferences

#### Core Entities & Aggregates

```csharp
// Aggregate Root
public class MealPlan
{
    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public string Name { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public List<PlannedMeal> Meals { get; private set; }
    public Guid CreatedBy { get; private set; }

    // Domain methods
    public void AddMeal(DateTime date, MealType type, Guid recipeId);
    public void RemoveMeal(Guid plannedMealId);
    public List<Ingredient> GetAllIngredients();
}

// Entity
public class PlannedMeal
{
    public Guid Id { get; private set; }
    public DateTime Date { get; private set; }
    public MealType Type { get; private set; }
    public Guid RecipeId { get; private set; }
    public int Servings { get; private set; }
}

// Aggregate Root
public class Recipe
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<Ingredient> Ingredients { get; private set; }
    public List<string> Instructions { get; private set; }
    public int PrepTimeMinutes { get; private set; }
    public int CookTimeMinutes { get; private set; }
    public int Servings { get; private set; }
    public RecipeCategory Category { get; private set; }
    public NutritionalInfo Nutrition { get; private set; }
}

// Value Objects
public enum MealType
{
    Breakfast,
    Lunch,
    Dinner,
    Snack
}

public enum RecipeCategory
{
    Appetizer,
    MainCourse,
    Dessert,
    Beverage,
    Soup,
    Salad
}

public record Ingredient(string Name, decimal Quantity, string Unit);

public record NutritionalInfo(
    int Calories,
    decimal ProteinGrams,
    decimal CarbsGrams,
    decimal FatGrams
);
```

#### Domain Events Published

```csharp
public record MealPlannedEvent(
    Guid MealPlanId,
    Guid FamilyGroupId,
    DateTime MealDate,
    MealType Type,
    Guid RecipeId,
    List<Ingredient> Ingredients
);

public record RecipeCreatedEvent(
    Guid RecipeId,
    string Name,
    RecipeCategory Category
);

public record ShoppingListRequestedFromMealPlanEvent(
    Guid MealPlanId,
    List<Ingredient> RequiredIngredients
);
```

#### Domain Events Consumed

```csharp
// From Shopping Service
public record ShoppingListCompletedEvent(
    Guid ListId,
    DateTime CompletedAt
);
```

#### GraphQL API Schema Outline

```graphql
type MealPlan {
  id: ID!
  familyGroup: FamilyGroup!
  name: String!
  startDate: Date!
  endDate: Date!
  meals: [PlannedMeal!]!
  createdBy: User!
}

type PlannedMeal {
  id: ID!
  date: Date!
  type: MealType!
  recipe: Recipe!
  servings: Int!
}

type Recipe {
  id: ID!
  name: String!
  description: String
  ingredients: [Ingredient!]!
  instructions: [String!]!
  prepTimeMinutes: Int!
  cookTimeMinutes: Int!
  servings: Int!
  category: RecipeCategory!
  nutrition: NutritionalInfo
}

type Ingredient {
  name: String!
  quantity: Decimal!
  unit: String!
}

type NutritionalInfo {
  calories: Int!
  proteinGrams: Decimal!
  carbsGrams: Decimal!
  fatGrams: Decimal!
}

type Query {
  mealPlans(familyGroupId: ID!): [MealPlan!]!
  mealPlan(id: ID!): MealPlan
  recipes(category: RecipeCategory): [Recipe!]!
  recipe(id: ID!): Recipe
}

type Mutation {
  createMealPlan(input: CreateMealPlanInput!): MealPlan!
  addMealToplan(planId: ID!, input: AddMealInput!): PlannedMeal!
  createRecipe(input: CreateRecipeInput!): Recipe!
  generateShoppingListFromMealPlan(planId: ID!): ShoppingList!
}
```

#### Storage Strategy

- **Primary Database:** PostgreSQL
- **Schema:** `meal_planning` schema
- **Tables:** `meal_plans`, `planned_meals`, `recipes`, `recipe_ingredients`
- **Caching:** Redis for frequently accessed recipes
- **Indexing:** Indexes on `(family_group_id, start_date)`, `(recipe.category)`

---

### 2.6 Health Service

**Type:** Supporting Domain
**Pattern:** DDD with privacy considerations

#### Core Responsibilities

- Medical appointment tracking
- Prescription management
- Health metrics and tracking
- Doctor and provider information
- Medical document storage references

#### Core Entities & Aggregates

```csharp
// Aggregate Root
public class HealthAppointment
{
    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public Guid PatientUserId { get; private set; }
    public string DoctorName { get; private set; }
    public string Specialty { get; private set; }
    public DateTime AppointmentDateTime { get; private set; }
    public string Location { get; private set; }
    public string Reason { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string Notes { get; private set; }

    // Domain methods
    public void Reschedule(DateTime newDateTime);
    public void Cancel(string reason);
    public void Complete(string notes);
}

// Aggregate Root
public class Prescription
{
    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public Guid PatientUserId { get; private set; }
    public string MedicationName { get; private set; }
    public string Dosage { get; private set; }
    public string Frequency { get; private set; }
    public DateTime IssuedDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public string PrescribedBy { get; private set; }
    public int RefillsRemaining { get; private set; }
    public bool IsActive { get; private set; }

    // Domain methods
    public void MarkRefilled();
    public void SetInactive();
}

// Value Objects
public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    Completed,
    Cancelled,
    NoShow
}
```

#### Domain Events Published

```csharp
public record HealthAppointmentScheduledEvent(
    Guid AppointmentId,
    Guid FamilyGroupId,
    Guid PatientUserId,
    string DoctorName,
    DateTime AppointmentTime,
    string Location
);

public record PrescriptionIssuedEvent(
    Guid PrescriptionId,
    Guid PatientUserId,
    string MedicationName,
    string Dosage,
    DateTime IssuedDate,
    int RefillsRemaining
);

public record PrescriptionRefillReminderEvent(
    Guid PrescriptionId,
    Guid PatientUserId,
    string MedicationName,
    DateTime RefillDate
);

public record AppointmentReminderEvent(
    Guid AppointmentId,
    Guid PatientUserId,
    DateTime AppointmentTime,
    int HoursUntilAppointment
);
```

#### Domain Events Consumed

```csharp
// From Calendar Service
public record CalendarEventCancelledEvent(
    Guid EventId,
    string Reason
);

// From Task Service
public record TaskCompletedEvent(
    Guid TaskId,
    Guid CompletedBy
);
```

#### GraphQL API Schema Outline

```graphql
type HealthAppointment {
  id: ID!
  familyGroup: FamilyGroup!
  patient: User!
  doctorName: String!
  specialty: String
  appointmentDateTime: DateTime!
  location: String!
  reason: String
  status: AppointmentStatus!
  notes: String
}

type Prescription {
  id: ID!
  familyGroup: FamilyGroup!
  patient: User!
  medicationName: String!
  dosage: String!
  frequency: String!
  issuedDate: Date!
  expiryDate: Date
  prescribedBy: String!
  refillsRemaining: Int!
  isActive: Boolean!
}

enum AppointmentStatus {
  SCHEDULED
  CONFIRMED
  COMPLETED
  CANCELLED
  NO_SHOW
}

type Query {
  healthAppointments(
    familyGroupId: ID!
    patientId: ID
    status: AppointmentStatus
  ): [HealthAppointment!]!

  prescriptions(
    familyGroupId: ID!
    patientId: ID
    isActive: Boolean
  ): [Prescription!]!

  upcomingAppointments(familyGroupId: ID!): [HealthAppointment!]!
}

type Mutation {
  scheduleAppointment(input: ScheduleAppointmentInput!): HealthAppointment!
  rescheduleAppointment(id: ID!, newDateTime: DateTime!): HealthAppointment!
  cancelAppointment(id: ID!, reason: String): HealthAppointment!

  createPrescription(input: CreatePrescriptionInput!): Prescription!
  markPrescriptionRefilled(id: ID!): Prescription!
  deactivatePrescription(id: ID!): Prescription!
}
```

#### Storage Strategy

- **Primary Database:** PostgreSQL with encryption at rest
- **Schema:** `health` schema
- **Tables:** `health_appointments`, `prescriptions`, `health_metrics`
- **Privacy:** Row-level security, encrypted sensitive fields
- **Caching:** Limited caching due to privacy concerns
- **Indexing:** Indexes on `(patient_user_id, appointment_date_time)`, `(family_group_id, status)`

---

### 2.7 Finance Service

**Type:** Supporting Domain
**Pattern:** DDD with transaction integrity

#### Core Responsibilities

- Budget management
- Expense tracking
- Income recording
- Financial goals
- Category-based reporting

#### Core Entities & Aggregates

```csharp
// Aggregate Root
public class Budget
{
    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public string Name { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public List<BudgetCategory> Categories { get; private set; }

    // Domain methods
    public void AddCategory(string name, decimal allocatedAmount);
    public void AdjustCategoryBudget(Guid categoryId, decimal newAmount);
    public decimal GetRemainingBudget();
}

// Entity
public class BudgetCategory
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal AllocatedAmount { get; private set; }
    public decimal SpentAmount { get; private set; }

    public decimal GetRemaining() => AllocatedAmount - SpentAmount;
}

// Aggregate Root
public class Expense
{
    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public Guid RecordedBy { get; private set; }
    public decimal Amount { get; private set; }
    public string Category { get; private set; }
    public string Description { get; private set; }
    public DateTime Date { get; private set; }
    public ExpenseType Type { get; private set; }
    public string Merchant { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
}

// Value Objects
public enum ExpenseType
{
    Groceries,
    Utilities,
    Healthcare,
    Transportation,
    Entertainment,
    Education,
    Housing,
    Other
}

public enum PaymentMethod
{
    Cash,
    CreditCard,
    DebitCard,
    BankTransfer,
    Other
}
```

#### Domain Events Published

```csharp
public record BudgetCreatedEvent(
    Guid BudgetId,
    Guid FamilyGroupId,
    string Name,
    decimal TotalAmount,
    DateTime StartDate,
    DateTime EndDate
);

public record ExpenseRecordedEvent(
    Guid ExpenseId,
    Guid FamilyGroupId,
    decimal Amount,
    string Category,
    DateTime Date
);

public record BudgetThresholdExceededEvent(
    Guid BudgetId,
    Guid CategoryId,
    string CategoryName,
    decimal AllocatedAmount,
    decimal SpentAmount,
    decimal PercentageUsed
);
```

#### Domain Events Consumed

```csharp
// From Shopping Service
public record ShoppingListCompletedEvent(
    Guid ListId,
    DateTime CompletedAt,
    int ItemCount
);
```

#### GraphQL API Schema Outline

```graphql
type Budget {
  id: ID!
  familyGroup: FamilyGroup!
  name: String!
  totalAmount: Decimal!
  startDate: Date!
  endDate: Date!
  categories: [BudgetCategory!]!
  remainingBudget: Decimal!
}

type BudgetCategory {
  id: ID!
  name: String!
  allocatedAmount: Decimal!
  spentAmount: Decimal!
  remaining: Decimal!
}

type Expense {
  id: ID!
  familyGroup: FamilyGroup!
  recordedBy: User!
  amount: Decimal!
  category: String!
  description: String!
  date: Date!
  type: ExpenseType!
  merchant: String
  paymentMethod: PaymentMethod!
}

type Query {
  budgets(familyGroupId: ID!): [Budget!]!
  activeBudget(familyGroupId: ID!): Budget

  expenses(
    familyGroupId: ID!
    startDate: Date!
    endDate: Date!
    category: String
  ): [Expense!]!

  expenseSummary(
    familyGroupId: ID!
    startDate: Date!
    endDate: Date!
  ): ExpenseSummary!
}

type Mutation {
  createBudget(input: CreateBudgetInput!): Budget!
  recordExpense(input: RecordExpenseInput!): Expense!
  updateBudgetCategory(
    budgetId: ID!
    categoryId: ID!
    newAmount: Decimal!
  ): BudgetCategory!
}
```

#### Storage Strategy

- **Primary Database:** PostgreSQL with ACID compliance
- **Schema:** `finance` schema
- **Tables:** `budgets`, `budget_categories`, `expenses`, `income_records`
- **Transactions:** Strict transactional boundaries for financial operations
- **Caching:** Redis for budget summaries
- **Indexing:** Indexes on `(family_group_id, date)`, `(category, date)`

---

### 2.8 Communication Service

**Type:** Generic Subdomain
**Pattern:** Simple notification service

#### Core Responsibilities

- Push notifications
- Email notifications
- In-app messaging
- Notification preferences
- Event-triggered communications

#### Core Entities & Aggregates

```csharp
// Aggregate Root
public class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; }

    // Domain methods
    public void MarkAsRead();
}

// Value Objects
public enum NotificationType
{
    CalendarReminder,
    TaskAssignment,
    TaskDue,
    AppointmentReminder,
    BudgetAlert,
    General
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}
```

#### Domain Events Published

```csharp
public record NotificationSentEvent(
    Guid NotificationId,
    Guid UserId,
    NotificationType Type,
    DateTime SentAt
);
```

#### Domain Events Consumed

```csharp
// All events from other services that require notifications
public record CalendarEventReminderDueEvent(...);
public record TaskDueDateApproachingEvent(...);
public record AppointmentReminderEvent(...);
public record BudgetThresholdExceededEvent(...);
// ... etc
```

#### GraphQL API Schema Outline

```graphql
type Notification {
  id: ID!
  user: User!
  title: String!
  message: String!
  type: NotificationType!
  priority: NotificationPriority!
  createdAt: DateTime!
  isRead: Boolean!
  readAt: DateTime
  metadata: JSON
}

type Query {
  myNotifications(isRead: Boolean): [Notification!]!
  unreadNotificationCount: Int!
}

type Mutation {
  markNotificationAsRead(id: ID!): Notification!
  markAllNotificationsAsRead: Boolean!
}

type Subscription {
  notificationReceived(userId: ID!): Notification!
}
```

#### Storage Strategy

- **Primary Database:** PostgreSQL
- **Schema:** `communication` schema
- **Tables:** `notifications`, `notification_preferences`, `message_templates`
- **Real-time:** Redis Pub/Sub for instant notifications
- **Caching:** Redis for unread notification counts
- **Indexing:** Indexes on `(user_id, is_read, created_at)`

---

### 2.9 Federation Service (Fediverse Protocol)

**Type:** Core Domain - **KEY DIFFERENTIATOR**
**Pattern:** Open Host Service / Published Language

#### Core Responsibilities

- Instance discovery and registration
- Cross-instance authentication and authorization
- Federated family group management
- Inter-instance event propagation
- Instance trust and moderation
- Protocol versioning and compatibility
- Instance health monitoring

#### Federation Protocol

Family Hub uses a **custom federation protocol** inspired by ActivityPub with family-specific extensions:

**Instance Types:**

1. **Self-Hosted Instances** - Full data sovereignty, runs on user infrastructure
2. **Cloud-Hosted Instances** - Managed service on any Kubernetes provider (not limited to Azure/AWS/GCP)
3. **Hybrid Instances** - Mix of self-hosted and cloud components

**Federation Features:**

- **Cross-Instance Family Groups** - Family members can be on different instances
- **Federated Calendar Sharing** - Share events across instances with granular permissions
- **Cross-Instance Notifications** - Real-time notifications across federated instances
- **Data Sovereignty** - Each instance controls its own data storage and retention
- **Instance Blocking** - Instance admins can block malicious instances
- **Content Filtering** - Family-level content filtering across instances

#### Core Entities & Aggregates

```csharp
// Aggregate Root
public class FederatedInstance
{
    public Guid Id { get; private set; }
    public string Domain { get; private set; }  // e.g., familyhub.example.com
    public string Name { get; private set; }
    public InstanceType Type { get; private set; }  // SelfHosted, CloudHosted
    public string PublicKey { get; private set; }
    public DateTime DiscoveredAt { get; private set; }
    public DateTime LastSeenAt { get; private set; }
    public InstanceStatus Status { get; private set; }
    public string ProtocolVersion { get; private set; }
    public List<InstanceCapability> Capabilities { get; private set; }

    // Domain methods
    public void UpdateHeartbeat();
    public void Block(string reason);
    public void Trust();
    public bool SupportsCapability(InstanceCapability capability);
}

// Aggregate Root
public class FederatedFamilyGroup
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Guid HomeInstanceId { get; private set; }  // Primary instance
    public List<FederatedMember> Members { get; private set; }
    public FederationPolicy Policy { get; private set; }

    // Domain methods
    public void AddMemberFromInstance(Guid userId, Guid instanceId, FamilyRole role);
    public void ShareCalendarWithInstance(Guid instanceId, SharingPermissions permissions);
    public void RevokeInstanceAccess(Guid instanceId);
}

// Entity
public class FederatedMember
{
    public Guid UserId { get; private set; }
    public Guid InstanceId { get; private set; }
    public FamilyRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public bool IsLocal { get; private set; }  // true if on home instance
}

// Value Objects
public enum InstanceType
{
    SelfHosted,
    CloudHosted,
    Hybrid
}

public enum InstanceStatus
{
    Active,
    Inactive,
    Blocked,
    Untrusted
}

public enum InstanceCapability
{
    CalendarSharing,
    TaskSharing,
    ShoppingListSharing,
    MealPlanSharing,
    HealthDataSharing,  // Restricted, requires explicit consent
    FinanceDataSharing,  // Restricted, requires explicit consent
    DirectMessaging,
    VideoCall
}

public class FederationPolicy
{
    public bool AllowCrossInstanceSharing { get; private set; }
    public List<InstanceCapability> EnabledCapabilities { get; private set; }
    public PrivacyLevel DefaultPrivacyLevel { get; private set; }
    public bool RequireExplicitApproval { get; private set; }
}
```

#### Domain Events Published

```csharp
// Instance lifecycle events
public record InstanceDiscoveredEvent(Guid InstanceId, string Domain, DateTime DiscoveredAt);
public record InstanceBlockedEvent(Guid InstanceId, string Reason, DateTime BlockedAt);
public record InstanceTrustedEvent(Guid InstanceId, DateTime TrustedAt);

// Federation events
public record FederatedFamilyGroupCreatedEvent(Guid GroupId, Guid HomeInstanceId, string Name);
public record MemberAddedFromInstanceEvent(Guid GroupId, Guid UserId, Guid InstanceId);
public record CrossInstanceSharingEnabledEvent(Guid GroupId, Guid TargetInstanceId, InstanceCapability Capability);

// Data synchronization events (propagated to federated instances)
public record FederatedCalendarEventSharedEvent(Guid EventId, Guid SourceInstanceId, List<Guid> TargetInstanceIds);
public record FederatedTaskAssignedEvent(Guid TaskId, Guid AssigneeUserId, Guid AssigneeInstanceId);
```

#### Domain Events Consumed

```csharp
// Local events that might need federation
public record CalendarEventCreatedEvent(...);  // May trigger federated sharing
public record TaskAssignedEvent(...);  // May assign to user on different instance
public record ShoppingListUpdatedEvent(...);  // May sync to federated family members
```

#### GraphQL API Schema Outline

```graphql
type FederatedInstance {
  id: ID!
  domain: String!
  name: String!
  type: InstanceType!
  status: InstanceStatus!
  protocolVersion: String!
  capabilities: [InstanceCapability!]!
  discoveredAt: DateTime!
  lastSeenAt: DateTime!
}

type FederatedFamilyGroup {
  id: ID!
  name: String!
  homeInstance: FederatedInstance!
  members: [FederatedMember!]!
  policy: FederationPolicy!
}

type FederatedMember {
  userId: ID!
  instance: FederatedInstance!
  role: FamilyRole!
  isLocal: Boolean!
  joinedAt: DateTime!
}

enum InstanceType {
  SELF_HOSTED
  CLOUD_HOSTED
  HYBRID
}

enum InstanceStatus {
  ACTIVE
  INACTIVE
  BLOCKED
  UNTRUSTED
}

enum InstanceCapability {
  CALENDAR_SHARING
  TASK_SHARING
  SHOPPING_LIST_SHARING
  MEAL_PLAN_SHARING
  HEALTH_DATA_SHARING
  FINANCE_DATA_SHARING
  DIRECT_MESSAGING
  VIDEO_CALL
}

type Query {
  # Instance discovery
  discoverInstance(domain: String!): FederatedInstance
  knownInstances(status: InstanceStatus): [FederatedInstance!]!

  # Federation management
  myFederatedFamilies: [FederatedFamilyGroup!]!
  instanceCapabilities(domain: String!): [InstanceCapability!]!
}

type Mutation {
  # Instance management
  trustInstance(domain: String!): FederatedInstance!
  blockInstance(instanceId: ID!, reason: String!): Boolean!

  # Federation management
  createFederatedFamily(
    name: String!
    policy: FederationPolicyInput!
  ): FederatedFamilyGroup!
  inviteMemberFromInstance(
    familyId: ID!
    userEmail: String!
    instanceDomain: String!
    role: FamilyRole!
  ): FederatedMember!
  enableCrossInstanceSharing(
    familyId: ID!
    instanceId: ID!
    capability: InstanceCapability!
  ): Boolean!
}

type Subscription {
  # Real-time federation updates
  instanceStatusChanged(instanceId: ID!): FederatedInstance!
  federatedEventReceived(familyId: ID!): FederatedEvent!
}
```

#### Federation REST API

In addition to GraphQL, Federation Service exposes REST endpoints for instance-to-instance communication:

```
POST   /federation/v1/instance/register       # Instance registration
GET    /federation/v1/instance/info           # Instance metadata
POST   /federation/v1/instance/heartbeat      # Health check

POST   /federation/v1/events/push             # Push federated events
GET    /federation/v1/events/pull             # Pull pending events

POST   /federation/v1/auth/verify-token       # Cross-instance token verification
POST   /federation/v1/auth/user-lookup        # Find user by email@instance

POST   /federation/v1/family/invite           # Send family invitation
POST   /federation/v1/family/accept           # Accept invitation
POST   /federation/v1/family/sync             # Sync family data
```

#### Storage Strategy

- **Primary Database:** PostgreSQL
- **Schema:** `federation` schema
- **Tables:**
  - `federated_instances` - Instance directory
  - `federated_family_groups` - Cross-instance family groups
  - `federated_members` - Family members from other instances
  - `instance_blocks` - Blocked instances
  - `pending_federation_events` - Event queue for async processing
- **Caching:** Redis for instance metadata and heartbeat status
- **Message Queue:** Redis Streams for cross-instance event processing
- **Indexing:**
  - Indexes on `(domain)`, `(status, last_seen_at)`, `(home_instance_id)`
- **Security:**
  - Public/private key pairs for instance authentication (Ed25519)
  - TLS 1.3 required for all inter-instance communication
  - Rate limiting per instance

#### Cloud-Agnostic Deployment Strategy

**Kubernetes-Based Deployment** enables hosting on ANY provider:

**Supported Hosting Options:**

1. **Self-Hosted**

   - Docker Compose (simple setup)
   - k3s (lightweight Kubernetes)
   - Full Kubernetes cluster

2. **Cloud Providers** (via Kubernetes)

   - DigitalOcean Kubernetes
   - Linode Kubernetes Engine
   - Hetzner Cloud
   - AWS EKS (if needed)
   - Azure AKS (if needed)
   - Google GKE (if needed)
   - Oracle Cloud
   - Scaleway
   - Any Kubernetes-compatible provider

3. **Managed Hosting Service** (Official)
   - Pre-configured instances on cloud-agnostic infrastructure
   - Automatic updates and backups
   - Geographic distribution options
   - Pay-as-you-go pricing

**Helm Chart Deployment:**

```yaml
# values.yaml
instance:
  type: "cloud-hosted" # or "self-hosted"
  domain: "familyhub.example.com"

federation:
  enabled: true
  protocol_version: "1.0"
  capabilities:
    - CALENDAR_SHARING
    - TASK_SHARING
    - SHOPPING_LIST_SHARING

hosting:
  provider: "auto-detect" # Works with any K8s
  storage_class: "standard" # Provider-agnostic
```

---

## 3. Event Chain Specifications

### 3.1 Doctor Appointment Event Chain

This is the flagship event chain demonstrating cross-domain automation.

```
┌─────────────────────────────────────────────────────────────────┐
│  Event Chain: Doctor Appointment → Calendar → Shopping → Task   │
└─────────────────────────────────────────────────────────────────┘

1. USER ACTION: Schedule doctor appointment in Health Service
   └─▶ HealthAppointmentScheduledEvent

2. Calendar Service consumes event
   └─▶ Creates CalendarEvent (type: MEDICAL)
   └─▶ CalendarEventCreatedEvent

3. Task Service consumes HealthAppointmentScheduledEvent
   └─▶ Creates Task: "Prepare questions for Dr. Smith"
   └─▶ TaskCreatedEvent

4. Communication Service consumes HealthAppointmentScheduledEvent
   └─▶ Sends NotificationSentEvent (24h before appointment)

5. [Optional] If prescription is issued after appointment
   └─▶ PrescriptionIssuedEvent

6. Shopping Service consumes PrescriptionIssuedEvent
   └─▶ Adds medication to shopping list
   └─▶ ShoppingItemAddedEvent

7. Task Service consumes PrescriptionIssuedEvent
   └─▶ Creates Task: "Pick up prescription for [medication]"
   └─▶ Sets due date based on prescription urgency
```

### 3.2 Meal Planning Event Chain

```
┌─────────────────────────────────────────────────────────────────┐
│  Event Chain: Meal Plan → Shopping List → Task → Budget         │
└─────────────────────────────────────────────────────────────────┘

1. USER ACTION: Create weekly meal plan
   └─▶ MealPlannedEvent (with ingredients list)

2. Shopping Service consumes MealPlannedEvent
   └─▶ Creates ShoppingList with all ingredients
   └─▶ ShoppingListCreatedEvent

3. Task Service consumes ShoppingListCreatedEvent
   └─▶ Creates Task: "Buy groceries for week"
   └─▶ TaskCreatedEvent

4. User completes shopping
   └─▶ ShoppingListCompletedEvent

5. Finance Service consumes ShoppingListCompletedEvent
   └─▶ Prompts for expense recording
   └─▶ ExpenseRecordedEvent

6. Finance Service checks budget
   └─▶ [If threshold exceeded] BudgetThresholdExceededEvent

7. Communication Service sends budget alert
```

### 3.3 Recurring Task Event Chain

```
┌─────────────────────────────────────────────────────────────────┐
│  Event Chain: Recurring Task → Calendar → Notification          │
└─────────────────────────────────────────────────────────────────┘

1. SYSTEM: Recurring task instance created
   └─▶ TaskCreatedEvent (with recurrence metadata)

2. Calendar Service consumes TaskCreatedEvent
   └─▶ Creates CalendarEvent for task deadline
   └─▶ CalendarEventCreatedEvent

3. SYSTEM: 24 hours before deadline
   └─▶ TaskDueDateApproachingEvent

4. Communication Service consumes event
   └─▶ Sends reminder notification
   └─▶ NotificationSentEvent

5. User completes task
   └─▶ TaskCompletedEvent

6. SYSTEM: Generates next recurrence instance
   └─▶ Loop back to step 1
```

---

## 4. Cross-Cutting Concerns

### 4.1 Event Bus Architecture

**Technology:** Redis Pub/Sub (Phase 1), RabbitMQ (Phase 2 optional)

**Event Structure:**

```csharp
public class DomainEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateName { get; set; }
    public DateTime OccurredAt { get; set; }
    public Guid TriggeredBy { get; set; }
    public string Payload { get; set; } // JSON serialized
    public Dictionary<string, string> Metadata { get; set; }
}
```

**Event Publishing Pattern:**

```csharp
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : DomainEvent;
}

public class RedisEventPublisher : IEventPublisher
{
    private readonly IConnectionMultiplexer _redis;

    public async Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : DomainEvent
    {
        var channel = $"events:{domainEvent.EventType}";
        var message = JsonSerializer.Serialize(domainEvent);
        await _redis.GetSubscriber().PublishAsync(channel, message);
    }
}
```

### 4.2 API Gateway Pattern

**Technology:** YARP (Yet Another Reverse Proxy) or Ocelot

**Responsibilities:**

- GraphQL federation (Apollo Gateway or Hot Chocolate Stitching)
- Authentication token validation
- Rate limiting
- Request routing
- CORS handling

**GraphQL Federation Schema:**

```graphql
# Gateway exposes unified schema
type Query {
  # Calendar Service
  calendarEvents(...): [CalendarEvent!]!

  # Task Service
  tasks(...): [Task!]!

  # Shopping Service
  shoppingLists(...): [ShoppingList!]!

  # ... etc
}

# Federation allows cross-service queries
type CalendarEvent {
  id: ID!
  title: String!
  relatedTasks: [Task!]! # Resolved via Task Service
}
```

### 4.3 Resilience Patterns

**Retry Policy:**

```csharp
services.AddHttpClient<ICalendarService, CalendarServiceClient>()
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
```

**Circuit Breaker:**

```csharp
services.AddHttpClient<IHealthService, HealthServiceClient>()
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1)));
```

**Event Replay:**

- All domain events stored in event store
- Support for event replay in case of processing failures
- Dead letter queue for failed events

---

## 5. Data Consistency Strategy

### 5.1 Eventual Consistency

Most cross-service operations use eventual consistency via events.

**Example: Calendar Event from Health Appointment**

1. Health Service commits appointment to database
2. Publishes HealthAppointmentScheduledEvent
3. Calendar Service consumes event (asynchronously)
4. Calendar Service creates CalendarEvent
5. System is eventually consistent

### 5.2 Saga Pattern for Critical Workflows

For operations requiring compensation logic:

**Example: Budget-aware Expense Recording**

```csharp
// Saga Coordinator
public class ExpenseRecordingSaga
{
    public async Task<Result> ExecuteAsync(RecordExpenseCommand command)
    {
        // Step 1: Record expense
        var expense = await _financeService.RecordExpenseAsync(command);

        // Step 2: Update budget category
        var budgetUpdate = await _financeService.UpdateCategorySpentAsync(
            command.BudgetCategoryId,
            command.Amount
        );

        if (!budgetUpdate.Success)
        {
            // Compensating action: Rollback expense
            await _financeService.DeleteExpenseAsync(expense.Id);
            return Result.Failure("Budget update failed");
        }

        // Step 3: Publish event
        await _eventPublisher.PublishAsync(new ExpenseRecordedEvent(...));

        return Result.Success();
    }
}
```

### 5.3 Distributed Transactions (Minimal Use)

Only for critical financial operations:

- Use two-phase commit sparingly
- Prefer saga pattern for most workflows

---

## 6. Technology Stack Mapping

| Layer                       | Technology               | Purpose                          |
| --------------------------- | ------------------------ | -------------------------------- |
| **API Gateway**             | YARP / Ocelot            | Request routing, auth validation |
| **GraphQL**                 | Hot Chocolate (C#)       | Unified API, type-safe queries   |
| **Backend Services**        | .NET Core 10 / C# 14     | Microservices implementation     |
| **Event Bus**               | Redis Pub/Sub            | Event-driven communication       |
| **Database**                | PostgreSQL 16            | Primary data store per service   |
| **Caching**                 | Redis 7                  | Performance optimization         |
| **Auth**                    | Zitadel                  | External identity provider       |
| **Container Orchestration** | Kubernetes               | Deployment, scaling, management  |
| **Frontend**                | Angular v21 + TypeScript | SPA with Tailwind CSS            |
| **Monitoring**              | Prometheus + Grafana     | Metrics and observability        |
| **Logging**                 | Seq / ELK Stack          | Centralized logging              |

---

## 7. Microservices Deployment Map

```yaml
# Kubernetes Namespace: family-hub

Services:
  - api-gateway:
      port: 80
      replicas: 2
      resources:
        requests: { cpu: 100m, memory: 128Mi }
        limits: { cpu: 500m, memory: 512Mi }

  - auth-service:
      port: 5001
      replicas: 1
      database: auth_db
      external: zitadel-integration

  - calendar-service:
      port: 5002
      replicas: 1
      database: calendar_db
      cache: redis

  - task-service:
      port: 5003
      replicas: 1
      database: tasks_db
      cache: redis

  - shopping-service:
      port: 5004
      replicas: 1
      database: shopping_db

  - health-service:
      port: 5005
      replicas: 1
      database: health_db (encrypted)

  - finance-service:
      port: 5006
      replicas: 1
      database: finance_db

  - meal-planning-service:
      port: 5007
      replicas: 1
      database: meal_planning_db

  - communication-service:
      port: 5008
      replicas: 1
      database: communication_db
      real-time: redis-pubsub

Databases:
  - PostgreSQL StatefulSet:
      replicas: 1 (Phase 1), 3 with replication (Phase 2)
      storage: 20Gi PVC

  - Redis:
      replicas: 1 (Phase 1), Redis Cluster (Phase 2)
      storage: 5Gi PVC

Frontend:
  - angular-app:
      port: 4200
      replicas: 2
      served-via: nginx
```

---

## 8. GraphQL Federation Strategy

### 8.1 Schema Stitching Approach

Each microservice exposes its own GraphQL schema. The API Gateway uses Hot Chocolate Schema Stitching to create a unified schema.

**Benefits:**

- Each service maintains autonomy
- Type-safe cross-service queries
- Single GraphQL endpoint for frontend

**Example Stitched Query:**

```graphql
query GetDashboard($familyGroupId: ID!) {
  # From Calendar Service
  upcomingEvents(familyGroupId: $familyGroupId, days: 7) {
    id
    title
    startTime
  }

  # From Task Service
  myTasks(status: NOT_STARTED) {
    id
    title
    dueDate
  }

  # From Shopping Service
  activeShoppingLists(familyGroupId: $familyGroupId) {
    id
    name
    items {
      name
      isPurchased
    }
  }
}
```

---

## 9. Data Sovereignty & Privacy

### 9.1 GDPR Compliance Considerations

- **Health Service:** Highest sensitivity, encryption at rest and in transit
- **Finance Service:** High sensitivity, transaction audit logs
- **Auth Service:** Personal data, right to deletion support
- **Other Services:** Standard protection measures

### 9.2 Data Isolation

- Each family group's data is logically isolated
- Row-level security (RLS) in PostgreSQL
- Service-level authorization checks

**Example RLS Policy:**

```sql
CREATE POLICY family_group_isolation ON calendar_events
  USING (family_group_id IN (
    SELECT family_group_id
    FROM family_members
    WHERE user_id = current_user_id()
  ));
```

---

## 10. Next Steps

This domain model and microservices architecture provides the foundation for implementation. The next document (Implementation Roadmap) will break this down into concrete phases optimized for single-developer execution.

**Key Validation Points:**

1. Review bounded context boundaries with stakeholders
2. Validate event chains match expected workflows
3. Confirm technology choices align with developer skillset
4. Verify deployment complexity is manageable

**Dependencies for Implementation:**

- Zitadel instance setup and configuration
- Kubernetes cluster (local Minikube for dev)
- PostgreSQL and Redis instances
- GraphQL tooling (Hot Chocolate NuGet packages)

---

**Document Status:** Ready for stakeholder review
**Next Document:** Implementation Roadmap with Phases
