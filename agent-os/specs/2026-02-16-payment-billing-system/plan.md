# Payment & Billing Module — Implementation Plan

## Context

FamilyHub needs a monetization strategy to sustain development. The product strategy defines a "Free & Open" philosophy (never gate privacy/core features behind paywall) with Freemium monetization. This plan covers the complete Payment module from domain model through frontend.

**Business model:** Freemium + Usage (per-family pricing, not per-seat)
**Target market:** Germany/DACH first (SEPA, Klarna, PayPal, Google/Apple Pay)
**Primary PSP:** Stripe (best .NET SDK, subscription billing, Customer Portal, Tax, SEPA)
**Pricing:** Free tier + Premium at 6.99 EUR/mo or 59.99 EUR/yr

---

## Task 1: Save Spec Documentation

Create `agent-os/specs/2026-02-16-payment-billing-system/` with all spec files (this folder).

---

## Task 2: Domain Model Foundation

- Create `Features/Payment/` folder structure
- Implement Vogen value objects: SubscriptionId, PaymentId, PlanId, UsageRecordId, SubscriptionStatus, BillingCycle, PlanTier, PaymentStatus, PaymentType, Money
- Implement aggregates:
  - `Subscription` — Family billing lifecycle (status, plan, cycle, trial, external IDs)
  - `Payment` — Individual transactions (amount, status, type, external ID)
  - `Plan` — Pricing tier definitions with features + limits (seeded data)
- Implement entities: PlanFeature, PlanLimit, UsageRecord
- Create repository interfaces: ISubscriptionRepository, IPaymentRepository, IPlanRepository, IUsageRecordRepository
- Create domain events: SubscriptionCreated/Activated/PlanChanged/Canceled/Expired/Renewed/PastDue, PaymentSucceeded/Failed/Refunded, UsageLimitApproaching/Exceeded

**Files:**

- `src/FamilyHub.Api/Features/Payment/Domain/ValueObjects/*.cs`
- `src/FamilyHub.Api/Features/Payment/Domain/Entities/*.cs`
- `src/FamilyHub.Api/Features/Payment/Domain/Events/*.cs`
- `src/FamilyHub.Api/Features/Payment/Domain/Repositories/*.cs`

---

## Task 3: PaymentModule + Infrastructure

- Create `PaymentModule : IModule` with DI registrations
- Register in `Program.cs`: `builder.Services.RegisterModule<PaymentModule>(configuration)`
- Add DbSets to `AppDbContext.cs`: Subscription, Payment, Plan, PlanFeature, PlanLimit, UsageRecord
- EF Core configurations for all entities in `Features/Payment/Data/`
- Create migration for `payment` schema
- Seed Free + Premium plan data

**Files:**

- `src/FamilyHub.Api/Features/Payment/PaymentModule.cs`
- `src/FamilyHub.Api/Program.cs` (1 line added)
- `src/FamilyHub.Api/Common/Database/AppDbContext.cs` (DbSets added)
- `src/FamilyHub.Api/Features/Payment/Data/*.cs`

---

## Task 4: Stripe Integration

- Implement `IPaymentGateway` interface (ACL between domain and Stripe SDK)
- Implement `StripePaymentGateway`: CreateSubscription, CreateCheckoutSession, CancelSubscription, CreateCustomerPortalSession, ProcessWebhook
- Configure `StripeConfiguration` from appsettings.json
- Implement webhook handler: `POST /api/webhooks/stripe` (REST, not GraphQL)
- Signature verification via `EventUtility.ConstructEvent()`
- Idempotency key tracking to prevent duplicate event processing

**Files:**

- `src/FamilyHub.Api/Features/Payment/Infrastructure/Gateway/IPaymentGateway.cs`
- `src/FamilyHub.Api/Features/Payment/Infrastructure/Gateway/StripePaymentGateway.cs`
- `src/FamilyHub.Api/Features/Payment/Infrastructure/Gateway/StripeConfiguration.cs`
- `src/FamilyHub.Api/Features/Payment/Infrastructure/Gateway/StripeWebhookHandler.cs`

---

## Task 5: Checkout + Subscription Commands

- `CreateCheckoutSession` — Creates Stripe Checkout session, returns redirect URL
- `CancelSubscription` — Cancels at end of billing period
- `ResumeSubscription` — Resumes canceled subscription (if still in period)
- `ChangeBillingCycle` — Switch monthly <-> yearly
- `ProcessPaymentSucceeded` — Webhook handler: marks payment succeeded, renews subscription
- `ProcessPaymentFailed` — Webhook handler: marks payment failed, sets subscription past-due
- `ProcessSubscriptionUpdated` — Webhook handler: syncs subscription status changes
- `BillingAuthorizationService` — Owner=manage, Admin=view

**Files:**

- `src/FamilyHub.Api/Features/Payment/Application/Commands/CreateCheckoutSession/`
- `src/FamilyHub.Api/Features/Payment/Application/Commands/CancelSubscription/`
- `src/FamilyHub.Api/Features/Payment/Application/Commands/ResumeSubscription/`
- `src/FamilyHub.Api/Features/Payment/Application/Commands/ChangeBillingCycle/`
- `src/FamilyHub.Api/Features/Payment/Application/Commands/ProcessPaymentSucceeded/`
- `src/FamilyHub.Api/Features/Payment/Application/Commands/ProcessPaymentFailed/`
- `src/FamilyHub.Api/Features/Payment/Application/Commands/ProcessSubscriptionUpdated/`
- `src/FamilyHub.Api/Features/Payment/Application/Services/BillingAuthorizationService.cs`

---

## Task 6: Queries + Feature Gating

- `GetSubscription` — Current family's subscription status
- `GetPlans` — Available pricing plans
- `GetUsage` — Current usage vs limits
- `GetBillingHistory` — Paginated payment history
- `GetCustomerPortalUrl` — Stripe Customer Portal redirect URL
- `IFeatureAccessService` — Checks feature/limit access for a family
- `FeatureAccessBehavior` — Pipeline behavior at priority 350 (between Validation and Transaction)
- `[RequiresFeature("key")]` + `[RequiresWithinLimit("key")]` attributes

**Files:**

- `src/FamilyHub.Api/Features/Payment/Application/Queries/GetSubscription/`
- `src/FamilyHub.Api/Features/Payment/Application/Queries/GetPlans/`
- `src/FamilyHub.Api/Features/Payment/Application/Queries/GetUsage/`
- `src/FamilyHub.Api/Features/Payment/Application/Queries/GetBillingHistory/`
- `src/FamilyHub.Api/Features/Payment/Application/Queries/GetCustomerPortalUrl/`
- `src/FamilyHub.Api/Features/Payment/Application/Services/FeatureAccessService.cs`
- `src/FamilyHub.Api/Common/Behaviors/FeatureAccessBehavior.cs`

---

## Task 7: Cross-Module Integration

- `FamilyCreatedEventHandler` — Auto-create Free subscription for new families
- `MemberAddedEventHandler` — Increment `max_family_members` usage counter
- `MemberRemovedEventHandler` — Decrement `max_family_members` usage counter
- `IUsageTrackingService` — Increment/decrement/query usage for any limit key
- `UsageLimitApproachingEvent` / `UsageLimitExceededEvent` handlers

**Files:**

- `src/FamilyHub.Api/Features/Payment/Application/EventHandlers/FamilyCreatedEventHandler.cs`
- `src/FamilyHub.Api/Features/Payment/Application/EventHandlers/MemberAddedEventHandler.cs`
- `src/FamilyHub.Api/Features/Payment/Application/EventHandlers/MemberRemovedEventHandler.cs`
- `src/FamilyHub.Api/Features/Payment/Application/Services/UsageTrackingService.cs`

---

## Task 8: Frontend Billing Feature

- Lazy-loaded `/billing` route with `provideBillingFeature()`
- Pricing page with plan comparison cards
- Billing overview (subscription status, usage meters)
- Billing history with pagination
- Checkout success/cancel pages
- `FeatureGateService` (signals-based feature checking)
- `SubscriptionStore` (signal-based subscription state)
- `BillingService` (Apollo GraphQL operations)
- `FeatureGateDirective` for template-level feature gating
- `UpgradePromptComponent` for reusable upgrade CTAs

**Files:**

- `src/frontend/family-hub-web/src/app/features/billing/`
- `src/frontend/family-hub-web/src/app/app.routes.ts` (1 route added)
- `src/frontend/family-hub-web/src/app/app.config.ts` (1 provider added)

---

## Task 9: Tests

- Domain aggregate tests (Subscription, Payment, Plan)
- Command handler tests with FakePaymentGateway
- Query handler tests
- FeatureAccessService tests
- UsageTrackingService tests
- BillingAuthorizationService tests
- Shared fakes: FakeSubscriptionRepository, FakePaymentRepository, FakePlanRepository, FakeUsageRecordRepository, FakePaymentGateway

**Files:**

- `tests/FamilyHub.Payment.Tests/`
- `tests/FamilyHub.TestCommon/Fakes/FakeSubscriptionRepository.cs`
- `tests/FamilyHub.TestCommon/Fakes/FakePaymentRepository.cs`
- `tests/FamilyHub.TestCommon/Fakes/FakePlanRepository.cs`
- `tests/FamilyHub.TestCommon/Fakes/FakeUsageRecordRepository.cs`
- `tests/FamilyHub.TestCommon/Fakes/FakePaymentGateway.cs`

---

## Implementation Sequencing

| Phase | Tasks | Duration | Dependencies |
|-------|-------|----------|--------------|
| 1A: Foundation | Task 2 + 3 | 2-3 weeks | None |
| 1B: Checkout | Task 4 + 5 | 2-3 weeks | 1A |
| 1C: Management | Task 6 | 1-2 weeks | 1B |
| 2: Frontend | Task 8 | 2-3 weeks | 1C |
| 3: Integration | Task 7 | 1-2 weeks | 1C |
| 4: Tests | Task 9 | 1-2 weeks | All above |

**Total estimated effort:** 9-15 weeks

---

## Pricing Structure

| | Free ("Family Essentials") | Premium ("Family Pro") |
|---|---|---|
| **Price** | 0 EUR | 6.99 EUR/mo or 59.99 EUR/yr |
| **Family members** | 5 | 15 |
| **Event chains** | 5 (basic) | Unlimited (advanced) |
| **Automations/month** | 50 | Unlimited |
| **Storage** | 100 MB | 5 GB |
| **Custom widgets** | 0 | 10 |
| **Trial** | -- | 14 days free |

**Annual discount:** 28% (5.00 EUR/mo effective)

---

## Payment Methods (Launch -> Future)

**Launch:** Credit/Debit Cards, SEPA Direct Debit, PayPal
**Phase 2 (3-6 months):** Klarna, Google Pay, Apple Pay
**Phase 3+ (12+ months):** Wero (when stable)
