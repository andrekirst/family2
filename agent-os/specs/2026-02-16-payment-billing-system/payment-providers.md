# Payment Provider Research — DACH Market

## PSP Comparison

| Criterion | Stripe | Mollie | Adyen | PAYONE |
|-----------|--------|--------|-------|--------|
| **.NET SDK** | Excellent (`Stripe.net` v50.3+, official, typed models, webhook utilities) | Good (`Mollie.Api` v4.17, community, covers v2 API) | Fair (official .NET library, less community adoption) | Poor (no SDK; SOAP/REST, manual HTTP wrapper) |
| **SEPA Direct Debit** | Yes (mandates API) | Yes (native, first-class) | Yes | Yes (strongest DACH native) |
| **Klarna** | Yes (requires activation per market) | Yes (native Orders API) | Yes | Yes |
| **Giropay/Wero** | Giropay deprecated; Wero not yet (Q3 2026 expected) | Giropay; Wero planned | Wero announced | Wero via EPI partnership |
| **PayPal** | Yes (Payment Element) | Yes (native) | Yes | Yes |
| **Google Pay / Apple Pay** | Yes (Payment Request API) | Yes | Yes | Yes |
| **Subscription Billing** | Excellent (plans, metered, trials, proration, invoicing, tax) | Limited (recurring profiles, no billing portal) | Good (tokenization + recurring engine) | Manual (dunning, no SaaS-grade engine) |
| **Webhooks** | Excellent (signature verification, retry, billing lifecycle events) | Good (verification, limited granularity) | Good | Fair |
| **Customer Portal** | Yes (hosted billing management) | No | No | No |
| **Tax Calculation** | Yes (Stripe Tax: EU VAT, reverse charge) | No | No | No |
| **PCI Level** | SAQ-A with Checkout, SAQ-A-EP with Elements | SAQ-A with hosted | SAQ-A with Drop-in | SAQ-A with hosted |
| **Self-serve Onboarding** | Instant (API keys in minutes) | Fast (few hours) | Slower (account review) | Slowest (contract negotiation) |

## Pricing Comparison

| Provider | Card (EEA) | SEPA DD | PayPal | Klarna | Monthly Fee |
|----------|-----------|---------|--------|--------|-------------|
| **Stripe** | 1.5% + 0.25 EUR | 0.35 EUR/mandate | 1.5% + 0.25 EUR | varies | None |
| Mollie | 1.8% + 0.25 EUR | 0.25 EUR/txn | 1.8% + 0.25 EUR | varies | None |
| Adyen | 0.60 EUR + scheme fee | 0.22 EUR + interchange | varies | varies | None (min volume) |
| PAYONE | Variable | Included | Variable | Variable | 18.90 EUR+ |

## Recommendation: Stripe as Primary PSP

### Why Stripe

1. **Subscription Billing is the core use case.** Stripe Billing is purpose-built for SaaS/freemium with trials, proration, dunning, and invoice generation. Mollie has nothing comparable. For a per-family subscription model, this eliminates months of custom billing logic.

2. **Stripe Customer Portal** provides a hosted page for families to manage subscriptions, update payment methods, and download invoices — reducing frontend development effort by 60-70% in Phase 1.

3. **Stripe Tax** auto-calculates German Umsatzsteuer (19% VAT), Austrian USt, and Swiss MwSt — critical for DACH multi-country compliance without building a tax engine.

4. **The .NET SDK** (`Stripe.net` v50.3+) is officially maintained, has typed models for every resource, provides `EventUtility.ConstructEvent` for webhook signature verification, and supports .NET Standard 2.0+.

5. **SEPA Direct Debit mandates** are first-class in Stripe, making the most popular German recurring payment method accessible from day one.

6. **PSD2/SCA compliance** is handled automatically — 3D Secure with automatic SCA exemption logic for recurring payments.

### Future: Mollie as Secondary

Add Mollie in Phase 3+ for Wero support once the EPI ecosystem matures. The `IPaymentGateway` abstraction layer enables this without domain model changes.

## Payment Methods by Phase

### Launch (Phase 1)

| Method | Provider | Market Share (DE) | Use Case |
|--------|----------|-------------------|----------|
| Credit/Debit Cards | Stripe | ~30% online | Universal fallback |
| SEPA Direct Debit | Stripe | ~25% recurring | Standard for German subscriptions |
| PayPal | Stripe | ~49% online | Most popular German payment method |

### Phase 2 (3-6 months post-launch)

| Method | Provider | Market Share (DE) | Use Case |
|--------|----------|-------------------|----------|
| Klarna | Stripe | ~6% BNPL | Pay Later / installments |
| Google Pay | Stripe | Growing mobile | Android users |
| Apple Pay | Stripe | Growing mobile | iOS users |

### Phase 3+ (12+ months)

| Method | Provider | Notes |
|--------|----------|-------|
| Wero | Mollie or Stripe | New European payment initiative, EPI-backed |
| Sofort | Stripe | If still relevant alongside Wero |

## Security & Compliance

### PCI DSS

- **Level: SAQ-A** (simplest) with Stripe Checkout
- Never store, process, or transmit raw card data
- All card handling on Stripe's servers
- Only tokenized references in our database

### PSD2 / SCA

- Stripe handles 3D Secure automatically
- First payment requires SCA, recurring exempt per EU regulation
- SEPA Direct Debit is SCA-exempt by design

### German Tax Compliance

- Stripe Tax auto-calculates 19% German USt/MwSt
- Reverse charge for B2B EU cross-border
- Stripe generates tax-compliant invoices

## Sources

- [Stripe — Payments in Germany](https://stripe.com/resources/more/payments-in-germany-an-in-depth-guide)
- [Top 20+ Payment Providers DACH](https://ecommercegermany.com/blog/top-20-payment-providers-in-the-dach-region/)
- [Payment Trends 2026 (Wero, A2A)](https://www.xictron.com/en/blog/payment-trends-wero-a2a-2026/)
- [Stripe Alternatives Europe 2026](https://www.european-saas.eu/blog/stripe-alternatives-europe)
- [Top Payment Gateways Europe 2026](https://colorwhistle.com/top-payment-gateways-europe/)
- [Stripe .NET SDK (NuGet)](https://www.nuget.org/packages/Stripe.net/)
- [Stripe Billing Documentation](https://docs.stripe.com/billing/subscriptions/build-subscriptions)
