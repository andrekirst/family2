# Payment & Billing — Database Schema

## PostgreSQL Schema: `payment`

All tables use the `payment` schema, following the per-module schema convention.

---

## Tables

### payment.plans

Seeded pricing tier definitions.

```sql
CREATE TABLE payment.plans (
    id              UUID PRIMARY KEY,
    plan_name       VARCHAR(100) NOT NULL,
    plan_tier       VARCHAR(50) NOT NULL,  -- 'Free', 'Premium'
    monthly_price_eur DECIMAL(10,2) NOT NULL DEFAULT 0,
    yearly_price_eur  DECIMAL(10,2) NOT NULL DEFAULT 0,
    trial_days      INT NOT NULL DEFAULT 0,
    external_price_id_monthly VARCHAR(255),  -- Stripe Price ID
    external_price_id_yearly  VARCHAR(255),  -- Stripe Price ID
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

**Note:** Plans table does NOT have RLS — plans are global and readable by all users.

### payment.plan_features

Feature flags per plan.

```sql
CREATE TABLE payment.plan_features (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    plan_id     UUID NOT NULL REFERENCES payment.plans(id) ON DELETE CASCADE,
    feature_key VARCHAR(100) NOT NULL,  -- e.g., 'advanced_event_chains'
    description VARCHAR(500)
);

CREATE INDEX ix_plan_features_plan_id ON payment.plan_features(plan_id);
CREATE UNIQUE INDEX ix_plan_features_plan_key ON payment.plan_features(plan_id, feature_key);
```

### payment.plan_limits

Usage limits per plan.

```sql
CREATE TABLE payment.plan_limits (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    plan_id     UUID NOT NULL REFERENCES payment.plans(id) ON DELETE CASCADE,
    limit_key   VARCHAR(100) NOT NULL,  -- e.g., 'max_family_members'
    limit_value INT NOT NULL,            -- -1 for unlimited
    UNIQUE(plan_id, limit_key)
);

CREATE INDEX ix_plan_limits_plan_id ON payment.plan_limits(plan_id);
```

### payment.subscriptions

Family subscription records.

```sql
CREATE TABLE payment.subscriptions (
    id                      UUID PRIMARY KEY,
    family_id               UUID NOT NULL,
    plan_id                 UUID NOT NULL REFERENCES payment.plans(id),
    status                  VARCHAR(50) NOT NULL,  -- 'Trialing','Active','PastDue','Canceled','Expired'
    billing_cycle           VARCHAR(20) NOT NULL,  -- 'Monthly','Yearly'
    current_period_start    TIMESTAMPTZ NOT NULL,
    current_period_end      TIMESTAMPTZ NOT NULL,
    trial_ends_at           TIMESTAMPTZ,
    canceled_at             TIMESTAMPTZ,
    cancellation_reason     TEXT,
    external_subscription_id VARCHAR(255),  -- Stripe subscription ID
    external_customer_id     VARCHAR(255),  -- Stripe customer ID
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at              TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX ix_subscriptions_family_id ON payment.subscriptions(family_id);
CREATE INDEX ix_subscriptions_status ON payment.subscriptions(status);
CREATE INDEX ix_subscriptions_external_id ON payment.subscriptions(external_subscription_id);
```

### payment.payments

Individual financial transactions.

```sql
CREATE TABLE payment.payments (
    id                      UUID PRIMARY KEY,
    subscription_id         UUID NOT NULL REFERENCES payment.subscriptions(id),
    family_id               UUID NOT NULL,
    amount                  DECIMAL(10,2) NOT NULL,
    currency_code           VARCHAR(3) NOT NULL DEFAULT 'EUR',
    status                  VARCHAR(50) NOT NULL,  -- 'Pending','Succeeded','Failed','Refunded','PartiallyRefunded'
    payment_type            VARCHAR(50) NOT NULL,  -- 'Subscription','OneTime','Refund'
    external_payment_id     VARCHAR(255),  -- Stripe payment_intent ID
    payment_method_summary  VARCHAR(100),  -- 'VISA ...4242' or 'SEPA DE89...12'
    failure_reason          TEXT,
    paid_at                 TIMESTAMPTZ,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_payments_subscription_id ON payment.payments(subscription_id);
CREATE INDEX ix_payments_family_id ON payment.payments(family_id);
CREATE INDEX ix_payments_external_id ON payment.payments(external_payment_id);
CREATE INDEX ix_payments_created_at ON payment.payments(created_at DESC);
```

### payment.usage_records

Current usage tracking per family per billing period.

```sql
CREATE TABLE payment.usage_records (
    id              UUID PRIMARY KEY,
    family_id       UUID NOT NULL,
    limit_key       VARCHAR(100) NOT NULL,
    current_usage   INT NOT NULL DEFAULT 0,
    period_start    TIMESTAMPTZ NOT NULL,
    period_end      TIMESTAMPTZ NOT NULL,
    last_updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX ix_usage_records_family_period ON payment.usage_records(family_id, limit_key, period_start);
CREATE INDEX ix_usage_records_family_id ON payment.usage_records(family_id);
```

### payment.webhook_idempotency

Tracks processed webhook event IDs to prevent duplicate processing.

```sql
CREATE TABLE payment.webhook_idempotency (
    event_id    VARCHAR(255) PRIMARY KEY,  -- Stripe event ID (evt_...)
    event_type  VARCHAR(100) NOT NULL,
    processed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Auto-cleanup: delete entries older than 30 days
CREATE INDEX ix_webhook_idempotency_processed_at ON payment.webhook_idempotency(processed_at);
```

---

## Row-Level Security

RLS enforces family-scoped data isolation. Following the existing pattern in `PostgresRlsMiddleware`.

```sql
-- Subscriptions: family-scoped
ALTER TABLE payment.subscriptions ENABLE ROW LEVEL SECURITY;
CREATE POLICY family_isolation ON payment.subscriptions
    USING (family_id = current_setting('app.current_family_id', true)::uuid);

-- Payments: family-scoped
ALTER TABLE payment.payments ENABLE ROW LEVEL SECURITY;
CREATE POLICY family_isolation ON payment.payments
    USING (family_id = current_setting('app.current_family_id', true)::uuid);

-- Usage records: family-scoped
ALTER TABLE payment.usage_records ENABLE ROW LEVEL SECURITY;
CREATE POLICY family_isolation ON payment.usage_records
    USING (family_id = current_setting('app.current_family_id', true)::uuid);
```

**Tables WITHOUT RLS (global read):**

- `payment.plans` — Pricing plans are public
- `payment.plan_features` — Features are public
- `payment.plan_limits` — Limits are public
- `payment.webhook_idempotency` — System-only table

**Webhook RLS bypass:** The webhook handler operates system-wide and must bypass RLS. Use a connection with the table owner role or a separate connection that doesn't set `app.current_family_id`.

---

## Seed Data

### Free Plan

```sql
INSERT INTO payment.plans (id, plan_name, plan_tier, monthly_price_eur, yearly_price_eur, trial_days, is_active)
VALUES ('00000000-0000-0000-0000-000000000001', 'Family Essentials', 'Free', 0.00, 0.00, 0, TRUE);

-- Features: none (free has no premium features)

-- Limits
INSERT INTO payment.plan_limits (plan_id, limit_key, limit_value) VALUES
('00000000-0000-0000-0000-000000000001', 'max_family_members', 5),
('00000000-0000-0000-0000-000000000001', 'max_event_chains', 5),
('00000000-0000-0000-0000-000000000001', 'max_automations_per_month', 50),
('00000000-0000-0000-0000-000000000001', 'max_storage_mb', 100),
('00000000-0000-0000-0000-000000000001', 'max_custom_widgets', 0);
```

### Premium Plan

```sql
INSERT INTO payment.plans (id, plan_name, plan_tier, monthly_price_eur, yearly_price_eur, trial_days,
    external_price_id_monthly, external_price_id_yearly, is_active)
VALUES ('00000000-0000-0000-0000-000000000002', 'Family Pro', 'Premium', 6.99, 59.99, 14,
    'price_monthly_xxx', 'price_yearly_xxx', TRUE);

-- Features
INSERT INTO payment.plan_features (plan_id, feature_key, description) VALUES
('00000000-0000-0000-0000-000000000002', 'advanced_event_chains', 'Advanced event chain builder with custom triggers and actions'),
('00000000-0000-0000-0000-000000000002', 'premium_widgets', 'Weather, meal planning, budget summary dashboard widgets'),
('00000000-0000-0000-0000-000000000002', 'export_billing_pdf', 'Export billing history as PDF'),
('00000000-0000-0000-0000-000000000002', 'priority_support', 'Priority email support');

-- Limits
INSERT INTO payment.plan_limits (plan_id, limit_key, limit_value) VALUES
('00000000-0000-0000-0000-000000000002', 'max_family_members', 15),
('00000000-0000-0000-0000-000000000002', 'max_event_chains', -1),  -- unlimited
('00000000-0000-0000-0000-000000000002', 'max_automations_per_month', -1),  -- unlimited
('00000000-0000-0000-0000-000000000002', 'max_storage_mb', 5120),  -- 5 GB
('00000000-0000-0000-0000-000000000002', 'max_custom_widgets', 10);
```

---

## EF Core Configuration

Each entity gets a configuration class in `Features/Payment/Data/`:

- `SubscriptionConfiguration.cs` — Schema "payment", indexes, Vogen converters
- `PaymentConfiguration.cs` — Schema "payment", Money as owned type or split columns
- `PlanConfiguration.cs` — Schema "payment", HasData() for seed
- `PlanFeatureConfiguration.cs` — FK to Plan
- `PlanLimitConfiguration.cs` — FK to Plan, unique constraint
- `UsageRecordConfiguration.cs` — Schema "payment", unique composite index

### Money Mapping

Map `Money` as two columns (not a separate table):

```csharp
builder.Property(p => p.Amount).HasColumnName("amount").HasPrecision(10, 2);
builder.Property(p => p.CurrencyCode).HasColumnName("currency_code").HasMaxLength(3);
```
