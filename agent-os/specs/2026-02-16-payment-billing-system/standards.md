# Standards for Payment & Billing System

The following 10 standards apply to this work.

---

## 1. ddd-modules

**Source:** `agent-os/standards/architecture/ddd-modules.md`
**Relevance:** Payment module follows bounded context pattern with feature-folder layout.

Key points:

- Module layout: `Domain/` (Entities, ValueObjects, Events, Repositories) + `Application/` (Commands, Queries, Services) + `Data/` (EF Core configs) + `Infrastructure/` (Repositories, Gateway)
- Cross-module: reference IDs only, no FK constraints across modules
- Event-driven cross-module communication via domain events
- One PostgreSQL schema per module: `payment`

---

## 2. graphql-input-command

**Source:** `agent-os/standards/backend/graphql-input-command.md`
**Relevance:** All billing mutations follow Input -> Command separation (ADR-003).

Key points:

- Input DTOs use primitives: `CreateCheckoutSessionInput { planId: string, billingCycle: string }`
- Commands use Vogen types: `CreateCheckoutSessionCommand(PlanId, BillingCycle, ...)`
- One `MutationType.cs` per command subfolder
- MutationType maps primitives to Vogen, dispatches via Mediator

---

## 3. permission-system

**Source:** `agent-os/standards/backend/permission-system.md`
**Relevance:** Billing access controlled by FamilyRole permissions.

Key points:

- New permissions: `billing:manage` (Owner), `billing:view` (Owner + Admin)
- Extend `FamilyRole` with: `CanManageBilling()`, `CanViewBilling()`
- `BillingAuthorizationService` follows `FamilyAuthorizationService` pattern
- Frontend: HIDE (not disable) unauthorized billing actions

---

## 4. vogen-value-objects

**Source:** `agent-os/standards/backend/vogen-value-objects.md`
**Relevance:** All IDs and domain values use Vogen with EfCoreValueConverter.

Key points:

- `[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]` for all ID types
- `[ValueObject<string>(...)]` for status/enum-like VOs with static factory members
- `Validate()` for business rules, `NormalizeInput()` for string normalization
- `Money` as a `sealed record` (composite value, not Vogen)

---

## 5. ef-core-migrations

**Source:** `agent-os/standards/database/ef-core-migrations.md`
**Relevance:** Payment schema migrations and configuration.

Key points:

- Schema: `builder.HasDefaultSchema("payment")`
- Migration in `Features/Payment/Data/` (following current project structure)
- Enable RLS in migration `Up()` method
- Test down migrations
- Seed data for Free + Premium plans via `HasData()`

---

## 6. rls-policies

**Source:** `agent-os/standards/database/rls-policies.md`
**Relevance:** Family-scoped data isolation for all payment tables.

Key points:

- Enable RLS on: `subscriptions`, `payments`, `usage_records`
- Policy: `USING (family_id = current_setting('app.current_family_id', true)::uuid)`
- NOT on: `plans`, `plan_features`, `plan_limits` (global read)
- Webhook handler must bypass RLS (system-wide operation)
- Middleware sets `app.current_family_id` session variable

---

## 7. domain-events

**Source:** `agent-os/standards/backend/domain-events.md`
**Relevance:** Subscription and payment lifecycle events.

Key points:

- Events are `sealed record : IDomainEvent`
- Past tense naming: `SubscriptionCreatedEvent`, `PaymentSucceededEvent`
- Raised via `RaiseDomainEvent()` on aggregates
- Handlers in `Application/EventHandlers/` as classes with `Handle()` method
- Cross-module: FamilyCreatedEvent -> auto-create Free subscription

---

## 8. angular-components

**Source:** `agent-os/standards/frontend/angular-components.md`
**Relevance:** All billing components are standalone with signals.

Key points:

- `standalone: true` on all components
- Angular Signals for reactive state
- Atomic design: pricing-page (page), plan-card (organism), usage-meter (molecule)
- `inject()` for DI (not constructor injection)

---

## 9. apollo-graphql

**Source:** `agent-os/standards/frontend/apollo-graphql.md`
**Relevance:** Typed GraphQL operations for billing queries/mutations.

Key points:

- `inject(Apollo)` for dependency injection
- GQL tagged templates for typed operations
- Error handling with `catchError`
- Separate `.graphql` files in `features/billing/graphql/`

---

## 10. unit-testing

**Source:** `agent-os/standards/testing/unit-testing.md`
**Relevance:** Payment handler tests with fake repositories + FakePaymentGateway.

Key points:

- xUnit + FluentAssertions
- Fake repository pattern: inner classes with in-memory state
- `FakePaymentGateway` implementing `IPaymentGateway` (configurable responses)
- Arrange-Act-Assert pattern
- Call handler methods directly with fakes
- Location: `tests/FamilyHub.Payment.Tests/`
