# Payment & Billing — GraphQL API

## Queries

### GetSubscription

Returns the current family's subscription status.

```graphql
type PaymentQuery {
  subscription: SubscriptionDto
}

type SubscriptionDto {
  id: ID!
  plan: PlanDto!
  status: SubscriptionStatus!
  billingCycle: BillingCycle!
  currentPeriodStart: DateTime!
  currentPeriodEnd: DateTime!
  trialEndsAt: DateTime
  canceledAt: DateTime
}
```

**Handler:** `GetSubscriptionQueryHandler`

- Requires authenticated user with family
- RLS scoped to current family
- Returns null if no subscription (shouldn't happen — auto-created on family creation)

### GetPlans

Returns all active pricing plans.

```graphql
type PaymentQuery {
  plans: [PlanDto!]!
}

type PlanDto {
  id: ID!
  name: String!
  tier: PlanTier!
  monthlyPriceEur: Decimal!
  yearlyPriceEur: Decimal!
  trialDays: Int!
  features: [String!]!
  limits: [PlanLimitDto!]!
}

type PlanLimitDto {
  key: String!
  label: String!
  maxValue: Int!
}
```

**Handler:** `GetPlansQueryHandler`

- Public (no auth required for pricing page)
- Returns only active plans
- Ordered: Free first, then Premium

### GetUsage

Returns current usage vs plan limits for the family.

```graphql
type PaymentQuery {
  usage: UsageSummaryDto!
}

type UsageSummaryDto {
  planTier: PlanTier!
  limits: [UsageLimitDto!]!
}

type UsageLimitDto {
  key: String!
  label: String!
  currentUsage: Int!
  maxValue: Int!
  percentUsed: Float!
}
```

**Handler:** `GetUsageQueryHandler`

- Requires authenticated user with family
- Joins plan limits with current usage records
- `percentUsed` = currentUsage / maxValue * 100 (capped at 100)
- `-1` maxValue = unlimited (percentUsed = 0)

### GetBillingHistory

Returns paginated payment history.

```graphql
type PaymentQuery {
  billingHistory(first: Int = 20, after: String): BillingHistoryConnection!
}

type BillingHistoryConnection {
  edges: [BillingHistoryEdge!]!
  pageInfo: PageInfo!
}

type BillingHistoryEdge {
  cursor: String!
  node: PaymentDto!
}

type PaymentDto {
  id: ID!
  amount: Decimal!
  currency: String!
  status: PaymentStatus!
  paymentType: PaymentType!
  paymentMethodSummary: String!
  paidAt: DateTime
  createdAt: DateTime!
}
```

**Handler:** `GetBillingHistoryQueryHandler`

- Requires authenticated user with `billing:view` permission (Owner or Admin)
- Cursor-based pagination (Hot Chocolate)
- Ordered by createdAt descending

### GetCustomerPortalUrl

Returns a Stripe Customer Portal URL for managing payment methods/invoices.

```graphql
type PaymentQuery {
  customerPortalUrl(returnUrl: String!): String!
}
```

**Handler:** `GetCustomerPortalUrlQueryHandler`

- Requires authenticated user with `billing:manage` permission (Owner only)
- Creates a Stripe Customer Portal session
- Returns temporary URL (valid for ~24 hours)

---

## Mutations

### CreateCheckoutSession

Creates a Stripe Checkout session for subscribing or upgrading.

```graphql
type PaymentMutation {
  createCheckoutSession(input: CreateCheckoutSessionInput!): CreateCheckoutSessionPayload!
}

input CreateCheckoutSessionInput {
  planId: ID!
  billingCycle: BillingCycle!
  successUrl: String!
  cancelUrl: String!
}

type CreateCheckoutSessionPayload {
  checkoutUrl: String!
  sessionId: String!
}
```

**Command:** `CreateCheckoutSessionCommand(FamilyId, PlanId, BillingCycle, string SuccessUrl, string CancelUrl)`
**Handler:** Creates Stripe Checkout Session via `IPaymentGateway.CreateCheckoutSessionAsync()`
**Auth:** Requires `billing:manage` permission (Owner only)
**Validation:**

- Plan must exist and be active
- Plan must be different from current (no re-subscribing to same plan)
- URLs must be valid

### CancelSubscription

Cancels at end of current billing period (not immediately).

```graphql
type PaymentMutation {
  cancelSubscription(input: CancelSubscriptionInput!): CancelSubscriptionPayload!
}

input CancelSubscriptionInput {
  reason: String
}

type CancelSubscriptionPayload {
  subscription: SubscriptionDto!
}
```

**Command:** `CancelSubscriptionCommand(FamilyId, string? Reason)`
**Handler:** Calls `IPaymentGateway.CancelSubscriptionAsync()`, updates local Subscription aggregate
**Auth:** Requires `billing:manage` permission (Owner only)
**Rules:**

- Only Active or Trialing subscriptions can be canceled
- Cancellation takes effect at period end (grace period)
- Free-tier subscriptions cannot be canceled

### ResumeSubscription

Resumes a previously canceled subscription (if still within current billing period).

```graphql
type PaymentMutation {
  resumeSubscription: ResumeSubscriptionPayload!
}

type ResumeSubscriptionPayload {
  subscription: SubscriptionDto!
}
```

**Command:** `ResumeSubscriptionCommand(FamilyId)`
**Handler:** Resumes via Stripe API, updates local Subscription aggregate
**Auth:** Requires `billing:manage` permission (Owner only)
**Rules:** Only canceled subscriptions that haven't expired yet can be resumed

### ChangeBillingCycle

Switch between monthly and yearly billing.

```graphql
type PaymentMutation {
  changeBillingCycle(input: ChangeBillingCycleInput!): ChangeBillingCyclePayload!
}

input ChangeBillingCycleInput {
  billingCycle: BillingCycle!
}

type ChangeBillingCyclePayload {
  subscription: SubscriptionDto!
}
```

**Command:** `ChangeBillingCycleCommand(FamilyId, BillingCycle)`
**Handler:** Updates Stripe subscription with new price, prorates
**Auth:** Requires `billing:manage` permission (Owner only)
**Rules:** Only active subscriptions, must be different from current cycle

---

## Enums

```graphql
enum SubscriptionStatus {
  TRIALING
  ACTIVE
  PAST_DUE
  CANCELED
  EXPIRED
}

enum BillingCycle {
  MONTHLY
  YEARLY
}

enum PlanTier {
  FREE
  PREMIUM
}

enum PaymentStatus {
  PENDING
  SUCCEEDED
  FAILED
  REFUNDED
  PARTIALLY_REFUNDED
}

enum PaymentType {
  SUBSCRIPTION
  ONE_TIME
  REFUND
}
```

---

## Webhook Endpoint (REST)

Webhooks are handled via a dedicated REST endpoint, **not** GraphQL. This is because:

- Stripe sends raw JSON payloads with signature headers
- No authentication (verified by webhook signature)
- Must return 200 quickly (Stripe retries on failure)

### Endpoint

```
POST /api/webhooks/stripe
Content-Type: application/json
Stripe-Signature: t=...,v1=...
```

### Mapped to Mediator Commands

| Stripe Event | Mediator Command |
|-------------|-----------------|
| `checkout.session.completed` | `ProcessCheckoutCompletedCommand` |
| `invoice.payment_succeeded` | `ProcessPaymentSucceededCommand` |
| `invoice.payment_failed` | `ProcessPaymentFailedCommand` |
| `customer.subscription.updated` | `ProcessSubscriptionUpdatedCommand` |
| `customer.subscription.deleted` | `ProcessSubscriptionDeletedCommand` |

### Registration in Program.cs

```csharp
app.MapPost("/api/webhooks/stripe", StripeWebhookHandler.HandleAsync)
    .AllowAnonymous();
```

### Webhook Handler Flow

1. Read raw request body
2. Verify signature via `EventUtility.ConstructEvent(body, signatureHeader, webhookSecret)`
3. Check idempotency (have we processed this event ID before?)
4. Map Stripe event type to Mediator command
5. Dispatch via `IMediator.Send()`
6. Return 200 OK (or 400 for invalid signature)

### RLS Bypass

Webhook handler must bypass RLS because it operates system-wide (not in a family context). Use a separate DbContext connection or bypass RLS via the connection owner role.
