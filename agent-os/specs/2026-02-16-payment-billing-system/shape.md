# Payment & Billing System — Shaping Notes

## Scope

Research payment providers for the DACH market + design complete domain architecture for a Freemium + Usage billing model. Per-family pricing (one subscription covers all family members). No implementation code — architecture spec only.

## Decisions

### Business Model

- **Freemium + Usage** — Free tier with generous limits, Premium tier unlocks advanced features
- **Per-family pricing** (like Cozi/FamilyWall), not per-seat (like Todoist/Notion)
- **Philosophy:** "Never gate privacy or core organization features behind paywall" (from PRODUCT_STRATEGY.md)

### Pricing

- **Free tier:** 5 family members, 5 event chains, 50 automations/month, 100 MB storage
- **Premium:** 6.99 EUR/month or 59.99 EUR/year (28% annual discount)
- **14-day free trial** of Premium (no credit card required to start)
- Competitive with FamilyWall ($5-8/mo) and Cozi Gold ($39/yr)

### Payment Provider

- **Primary: Stripe** — Best .NET SDK, subscription billing engine, Customer Portal, Tax (EU VAT), SEPA mandates, PCI SAQ-A with Checkout
- **Future secondary: Mollie** — For Wero support once EPI ecosystem matures
- **Rejected:** Adyen (slower onboarding, overkill for startup), PAYONE (no .NET SDK, SOAP-era)

### Payment Methods (DACH-first)

- **Launch:** Credit/Debit Cards (Visa, Mastercard), SEPA Direct Debit, PayPal
- **Phase 2:** Klarna (BNPL), Google Pay, Apple Pay
- **Phase 3+:** Wero (new European payment initiative, expected late 2026)

### Checkout Flow

- **Stripe Checkout (hosted)** for Phase 1 — zero PCI scope, automatic German localization, built-in SEPA mandate collection, SCA/3D Secure handling
- Saves 2-3 weeks vs custom payment form with Stripe Elements
- Consider embedded Elements in Phase 3+ for deeper brand customization

### Feature Gating

- **Backend:** `FeatureAccessBehavior` pipeline behavior at priority 350 (between Validation@300 and Transaction@400)
- **Attributes:** `[RequiresFeature("key")]`, `[RequiresWithinLimit("key")]` on command/query handlers
- **Frontend:** `FeatureGateService` (signals), `FeatureGateDirective` for templates — UX only, backend is authoritative
- Frontend hides (not disables) gated features, shows upgrade prompts

### Architecture

- Follows IModule pattern: `PaymentModule : IModule`
- PostgreSQL schema: `payment`
- Anti-corruption layer: `IPaymentGateway` isolates Stripe SDK from domain
- Cross-module: shared `FamilyId` VO + domain events (no direct entity references)
- Webhooks via REST endpoint (not GraphQL): `POST /api/webhooks/stripe`

## Competitor Research

| App | Model | Price | Billing Unit | Notes |
|-----|-------|-------|-------------|-------|
| **Todoist** | Per-user | Free / $4/mo Pro / $6/mo Business | Per user | No family plan |
| **Notion** | Per-seat | Free / $10/mo Plus / $15/mo Business | Per seat | Team/workspace plans |
| **Cozi** | Per-family | Free / $39/yr Gold | Per family | Simple Free+Gold model |
| **FamilyWall** | Per-family | Free / $5-8/mo or $45/yr Premium | Per family | 30-day free trial |

**Key insight:** Family apps charge per-family (value unit = household), productivity apps charge per-seat (value unit = individual). FamilyHub should follow per-family.

## Context

- **Visuals:** Dribbble pricing UI reference (provided by user)
- **References:** FamilyModule, AuthModule, FamilyAuthorizationService, FamilyRole, AppDbContext, Program.cs
- **Product alignment:** Matches PRODUCT_STRATEGY.md Section 8 (Pricing Strategy) — Free & Open core + optional Freemium Premium
- **Roadmap position:** Phase 3+ (months 13-24+), RICE score 40.0, estimated 5 weeks effort

## Standards Applied

1. **ddd-modules** — Payment module follows bounded context pattern
2. **graphql-input-command** — Checkout/Cancel mutations with Input->Command separation
3. **permission-system** — Owner=manage billing, Admin=view billing
4. **vogen-value-objects** — All IDs and domain values as Vogen VOs
5. **ef-core-migrations** — Payment schema with separate migrations
6. **rls-policies** — Family-scoped data isolation for all payment tables
7. **domain-events** — Subscription and payment lifecycle events
8. **angular-components** — Standalone components with signals
9. **apollo-graphql** — Typed GraphQL operations for billing
10. **unit-testing** — Fake repositories + FakePaymentGateway
