# Payment & Billing — Domain Model

## Overview

The Payment bounded context contains three aggregates (Subscription, Payment, Plan), three entities (PlanFeature, PlanLimit, UsageRecord), and twelve domain events. It follows the same patterns as the existing Family module.

---

## Aggregates

### Subscription (AggregateRoot)

The central aggregate representing a family's billing relationship.

```
Subscription (AggregateRoot<SubscriptionId>)
  Properties:
    FamilyId            (shared VO from FamilyHub.Common)
    PlanId              (VO)
    SubscriptionStatus  (VO: Trialing, Active, PastDue, Canceled, Expired)
    BillingCycle        (VO: Monthly, Yearly)
    CurrentPeriodStart  (DateTime)
    CurrentPeriodEnd    (DateTime)
    TrialEndsAt         (DateTime?)
    CanceledAt          (DateTime?)
    CancellationReason  (string?)
    ExternalSubscriptionId (string — Stripe subscription ID)
    ExternalCustomerId     (string — Stripe customer ID)
    CreatedAt           (DateTime)
    UpdatedAt           (DateTime)

  Domain Methods:
    static Create(FamilyId, PlanId, BillingCycle, ExternalSubscriptionId, ExternalCustomerId)
    Activate()
    StartTrial(DateTime trialEndsAt)
    MarkPastDue()
    Cancel(string reason)
    Renew(DateTime newPeriodStart, DateTime newPeriodEnd)
    ChangePlan(PlanId newPlanId)
    Expire()

  Domain Events:
    SubscriptionCreatedEvent
    SubscriptionActivatedEvent
    SubscriptionPlanChangedEvent
    SubscriptionCanceledEvent
    SubscriptionExpiredEvent
    SubscriptionRenewedEvent
    SubscriptionPastDueEvent
```

### Payment (AggregateRoot)

Individual financial transactions. Immutable after creation except for status updates from webhooks.

```
Payment (AggregateRoot<PaymentId>)
  Properties:
    SubscriptionId       (VO)
    FamilyId             (shared VO)
    Amount               (Money VO: decimal Value + string CurrencyCode)
    PaymentStatus        (VO: Pending, Succeeded, Failed, Refunded, PartiallyRefunded)
    PaymentType          (VO: Subscription, OneTime, Refund)
    ExternalPaymentId    (string — Stripe charge/payment_intent ID)
    PaymentMethodSummary (string — "VISA ...4242" or "SEPA DE89...12")
    FailureReason        (string?)
    PaidAt               (DateTime?)
    CreatedAt            (DateTime)

  Domain Methods:
    static Create(SubscriptionId, FamilyId, Money, PaymentType, ExternalPaymentId)
    MarkSucceeded(DateTime paidAt)
    MarkFailed(string reason)
    MarkRefunded(Money refundAmount)

  Domain Events:
    PaymentSucceededEvent
    PaymentFailedEvent
    PaymentRefundedEvent
```

### Plan (AggregateRoot — read-mostly, seeded)

Defines available pricing tiers. Changed only by administrators.

```
Plan (AggregateRoot<PlanId>)
  Properties:
    PlanName              (VO: string)
    PlanTier              (VO: Free, Premium)
    MonthlyPriceEur       (decimal)
    YearlyPriceEur        (decimal)
    TrialDays             (int)
    Features              (ICollection<PlanFeature>)
    Limits                (ICollection<PlanLimit>)
    ExternalPriceIdMonthly (string — Stripe Price ID)
    ExternalPriceIdYearly  (string — Stripe Price ID)
    IsActive              (bool)
    CreatedAt             (DateTime)
```

---

## Entities

### PlanFeature

```
PlanFeature (Entity)
  PlanFeatureId  (Guid — auto-generated)
  PlanId         (FK)
  FeatureKey     (string: "advanced_event_chains", "premium_widgets", etc.)
  Description    (string)
```

### PlanLimit

```
PlanLimit (Entity)
  PlanLimitId  (Guid — auto-generated)
  PlanId       (FK)
  LimitKey     (string: "max_family_members", "max_automations_per_month", "max_storage_mb")
  LimitValue   (int — use -1 for unlimited)
```

### UsageRecord

Tracked per family per billing period. Not a full aggregate.

```
UsageRecord (Entity)
  UsageRecordId   (VO)
  FamilyId        (shared VO)
  LimitKey        (string)
  CurrentUsage    (int)
  PeriodStart     (DateTime)
  PeriodEnd       (DateTime)
  LastUpdatedAt   (DateTime)
```

---

## Value Objects (Vogen)

All follow the Vogen pattern with `EfCoreValueConverter`:

```csharp
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct SubscriptionId { }

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct PaymentId { }

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct PlanId { }

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct UsageRecordId { }

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct SubscriptionStatus
{
    public static SubscriptionStatus Trialing => From("Trialing");
    public static SubscriptionStatus Active => From("Active");
    public static SubscriptionStatus PastDue => From("PastDue");
    public static SubscriptionStatus Canceled => From("Canceled");
    public static SubscriptionStatus Expired => From("Expired");
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct BillingCycle
{
    public static BillingCycle Monthly => From("Monthly");
    public static BillingCycle Yearly => From("Yearly");
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct PlanTier
{
    public static PlanTier Free => From("Free");
    public static PlanTier Premium => From("Premium");
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct PaymentStatus
{
    public static PaymentStatus Pending => From("Pending");
    public static PaymentStatus Succeeded => From("Succeeded");
    public static PaymentStatus Failed => From("Failed");
    public static PaymentStatus Refunded => From("Refunded");
    public static PaymentStatus PartiallyRefunded => From("PartiallyRefunded");
}

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct PaymentType
{
    public static PaymentType Subscription => From("Subscription");
    public static PaymentType OneTime => From("OneTime");
    public static PaymentType Refund => From("Refund");
}

// Composite value (not Vogen)
public sealed record Money(decimal Amount, string CurrencyCode)
{
    public static Money Eur(decimal amount) => new(amount, "EUR");
}
```

---

## Domain Events

All follow the existing `sealed record : IDomainEvent` pattern:

### Subscription Events

```csharp
public sealed record SubscriptionCreatedEvent(
    SubscriptionId SubscriptionId, FamilyId FamilyId, PlanId PlanId,
    BillingCycle Cycle, DateTime CreatedAt) : IDomainEvent;

public sealed record SubscriptionActivatedEvent(
    SubscriptionId SubscriptionId, FamilyId FamilyId, PlanId PlanId) : IDomainEvent;

public sealed record SubscriptionPlanChangedEvent(
    SubscriptionId SubscriptionId, FamilyId FamilyId,
    PlanId OldPlanId, PlanId NewPlanId) : IDomainEvent;

public sealed record SubscriptionCanceledEvent(
    SubscriptionId SubscriptionId, FamilyId FamilyId,
    string Reason, DateTime CanceledAt) : IDomainEvent;

public sealed record SubscriptionExpiredEvent(
    SubscriptionId SubscriptionId, FamilyId FamilyId) : IDomainEvent;

public sealed record SubscriptionRenewedEvent(
    SubscriptionId SubscriptionId, FamilyId FamilyId,
    DateTime NewPeriodStart, DateTime NewPeriodEnd) : IDomainEvent;

public sealed record SubscriptionPastDueEvent(
    SubscriptionId SubscriptionId, FamilyId FamilyId) : IDomainEvent;
```

### Payment Events

```csharp
public sealed record PaymentSucceededEvent(
    PaymentId PaymentId, SubscriptionId SubscriptionId,
    FamilyId FamilyId, Money Amount) : IDomainEvent;

public sealed record PaymentFailedEvent(
    PaymentId PaymentId, SubscriptionId SubscriptionId,
    FamilyId FamilyId, string FailureReason) : IDomainEvent;

public sealed record PaymentRefundedEvent(
    PaymentId PaymentId, FamilyId FamilyId, Money RefundAmount) : IDomainEvent;
```

### Usage Events

```csharp
public sealed record UsageLimitApproachingEvent(
    FamilyId FamilyId, string LimitKey,
    int CurrentUsage, int MaxUsage) : IDomainEvent;

public sealed record UsageLimitExceededEvent(
    FamilyId FamilyId, string LimitKey,
    int CurrentUsage, int MaxUsage) : IDomainEvent;
```

---

## Anti-Corruption Layers

### Payment <-> Family

- **Shared IDs only:** `FamilyId` is a shared VO in `FamilyHub.Common`. Payment stores it as a reference, never as a navigation property to Family aggregate.
- **Events (Family -> Payment):**
  - `FamilyCreatedEvent` -> Payment creates Free-tier subscription
  - `MemberAddedToFamilyEvent` -> Payment increments usage counter
  - `MemberRemovedFromFamilyEvent` -> Payment decrements usage counter
- **Events (Payment -> Family/Other):**
  - `SubscriptionExpiredEvent` -> Modules degrade to Free features
  - `SubscriptionActivatedEvent` -> Modules unlock Premium features
- **Query Interface:** `IPaymentFamilyLookupService` (thin interface for family display name on invoices)

### Payment <-> Stripe

- **`IPaymentGateway` interface** isolates Stripe SDK types from domain layer
- Domain layer knows nothing about Stripe
- Gateway translates between domain types and Stripe API types
- Enables swapping PSP without touching domain/application code

```
Domain Layer              Application Layer              Infrastructure Layer
Subscription       <---   IPaymentGateway          --->  StripePaymentGateway
  .Activate()              .CreateSubscription()          Stripe.SubscriptionService
  .Cancel()                .CancelSubscription()          Stripe.SubscriptionService
Payment                    .CreateCheckoutSession()       Stripe.Checkout.SessionService
  .MarkSucceeded() <---   .ProcessWebhookEvent()  --->  EventUtility.ConstructEvent()
```

---

## Feature Gating

### Backend: FeatureAccessBehavior (Pipeline Priority 350)

```csharp
[PipelinePriority(350)]
public sealed class FeatureAccessBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    // Checks [RequiresFeature] and [RequiresWithinLimit] attributes
    // Throws FeatureNotAvailableException if gated
}
```

### Attribute Decorators

```csharp
[RequiresFeature("advanced_event_chains")]
public sealed record CreateAdvancedChainCommand(...) : ICommand<...>;

[RequiresWithinLimit("max_family_members")]
public sealed record AcceptInvitationCommand(...) : ICommand<...>;
```

### IFeatureAccessService

```csharp
public interface IFeatureAccessService
{
    Task<bool> HasFeatureAsync(FamilyId familyId, string featureKey, CancellationToken ct);
    Task<bool> IsWithinLimitAsync(FamilyId familyId, string limitKey, CancellationToken ct);
    Task<FeatureAccessInfo> GetAccessInfoAsync(FamilyId familyId, CancellationToken ct);
}
```

### IUsageTrackingService

```csharp
public interface IUsageTrackingService
{
    Task IncrementAsync(FamilyId familyId, string limitKey, CancellationToken ct);
    Task DecrementAsync(FamilyId familyId, string limitKey, CancellationToken ct);
    Task<int> GetCurrentUsageAsync(FamilyId familyId, string limitKey, CancellationToken ct);
}
```

---

## File Structure

```
src/FamilyHub.Api/Features/Payment/
  PaymentModule.cs
  Domain/
    Entities/
      Subscription.cs
      Payment.cs
      Plan.cs
      PlanFeature.cs
      PlanLimit.cs
      UsageRecord.cs
    ValueObjects/
      SubscriptionId.cs, PaymentId.cs, PlanId.cs, UsageRecordId.cs
      SubscriptionStatus.cs, BillingCycle.cs, PlanTier.cs
      PaymentStatus.cs, PaymentType.cs, Money.cs
    Events/
      Subscription*Event.cs (7 events)
      Payment*Event.cs (3 events)
      UsageLimit*Event.cs (2 events)
    Repositories/
      ISubscriptionRepository.cs, IPaymentRepository.cs
      IPlanRepository.cs, IUsageRecordRepository.cs
  Application/
    Commands/ (7 command subfolders)
    Queries/ (5 query subfolders)
    EventHandlers/ (3 handlers)
    Services/
      BillingAuthorizationService.cs
      FeatureAccessService.cs
      UsageTrackingService.cs
  Data/ (6 EF Core configuration files)
  GraphQL/
    PaymentQueries.cs, PaymentMutations.cs
  Infrastructure/
    Repositories/ (4 implementations)
    Gateway/
      IPaymentGateway.cs, StripePaymentGateway.cs
      StripeConfiguration.cs, StripeWebhookHandler.cs
```
