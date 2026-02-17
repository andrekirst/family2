# References for Payment & Billing System

## Similar Implementations in Codebase

### FamilyModule (IModule Pattern)

- **Location:** `src/FamilyHub.Api/Features/Family/FamilyModule.cs`
- **Relevance:** Direct template for `PaymentModule` — same registration pattern
- **Key patterns:**
  - Scoped repository registration: `services.AddScoped<IFamilyRepository, FamilyRepository>()`
  - Authorization service registration: `services.AddScoped<FamilyAuthorizationService>()`
  - Configuration binding: `services.Configure<EmailConfiguration>(configuration.GetSection("Email"))`

### Family Aggregate (AggregateRoot Pattern)

- **Location:** `src/FamilyHub.Api/Features/Family/Domain/Entities/Family.cs`
- **Relevance:** Template for Subscription and Payment aggregates
- **Key patterns:**
  - Static factory method: `Family.Create(FamilyName, UserId)`
  - Domain event raising: `RaiseDomainEvent(new FamilyCreatedEvent(...))`
  - Vogen value object IDs
  - Guard clauses in domain methods

### CreateFamilyCommandHandler (Command Handler Pattern)

- **Location:** `src/FamilyHub.Api/Features/Family/Application/Commands/CreateFamily/`
- **Relevance:** Template for CreateCheckoutSession, CancelSubscription handlers
- **Key patterns:**
  - Subfolder layout: `Command.cs`, `Handler.cs`, `Validator.cs`, `MutationType.cs`
  - `ICommandHandler<TCommand, TResult>` interface
  - `ValueTask<TResult>` return type
  - Repository injection via constructor

### FamilyAuthorizationService

- **Location:** `src/FamilyHub.Api/Features/Family/Application/Services/FamilyAuthorizationService.cs`
- **Relevance:** Template for `BillingAuthorizationService`
- **Key patterns:**
  - Member lookup by UserId + FamilyId
  - Role-based permission check: `member?.Role.CanInvite() ?? false`
  - Used in handlers to throw `DomainException` when unauthorized

### FamilyRole (Permission System)

- **Location:** `src/FamilyHub.Api/Features/Family/Domain/ValueObjects/FamilyRole.cs`
- **Relevance:** Extend with billing permissions: `CanManageBilling()`, `CanViewBilling()`
- **Key patterns:**
  - `Can{Action}()` methods returning `bool`
  - `GetPermissions()` returning `List<string>` with `{module}:{action}` format
  - New permissions: `"billing:manage"`, `"billing:view"`

### AppDbContext (Database Pattern)

- **Location:** `src/FamilyHub.Api/Common/Database/AppDbContext.cs`
- **Relevance:** Add Payment module DbSets
- **Key patterns:**
  - Single shared DbContext (not per-module)
  - DbSet properties: `public DbSet<Subscription> Subscriptions { get; set; }`
  - `DomainEventInterceptor` for event publishing
  - `UpdateTimestamps()` for automatic CreatedAt/UpdatedAt

### Program.cs (Module Registration)

- **Location:** `src/FamilyHub.Api/Program.cs`
- **Relevance:** Add `RegisterModule<PaymentModule>()` + webhook endpoint
- **Key patterns:**
  - Explicit ordering: Auth -> Family -> Calendar -> EventChain -> **Payment**
  - Webhook endpoint: `app.MapPost("/api/webhooks/stripe", StripeWebhookHandler.HandleAsync).AllowAnonymous()`

### FamilyPermissionService (Frontend Permissions)

- **Location:** `src/frontend/family-hub-web/src/app/core/permissions/family-permission.service.ts`
- **Relevance:** Template for `FeatureGateService` (signal-based feature access checking)
- **Key patterns:**
  - `inject()` for DI
  - Computed signals for permission checks
  - HIDE (not disable) pattern for unauthorized UI

### Shared Fakes (Test Infrastructure)

- **Location:** `tests/FamilyHub.TestCommon/Fakes/`
- **Relevance:** Add FakeSubscriptionRepository, FakePaymentRepository, FakePlanRepository, FakePaymentGateway
- **Key patterns:**
  - In-memory state: `public List<T> Added { get; } = []`
  - `SaveChangesCalled` tracking
  - Configurable return values via constructor

## External References

### Stripe Documentation

- [Stripe Billing Subscriptions](https://docs.stripe.com/billing/subscriptions/build-subscriptions)
- [Stripe Checkout (Hosted)](https://docs.stripe.com/checkout)
- [Stripe Customer Portal](https://docs.stripe.com/customer-management/portal-deep-dive)
- [Stripe SEPA Direct Debit](https://docs.stripe.com/billing/subscriptions/sepa-debit)
- [Stripe Klarna Subscriptions](https://docs.stripe.com/billing/subscriptions/klarna)
- [Stripe Webhooks](https://docs.stripe.com/billing/subscriptions/webhooks)
- [Stripe .NET SDK (NuGet)](https://www.nuget.org/packages/Stripe.net/)
- [Stripe Tax (EU VAT)](https://docs.stripe.com/tax)

### Competitor Pricing

- [Todoist Pricing](https://www.todoist.com/pricing) — Free/$4/$6 per-user
- [Notion Pricing](https://www.notion.com/pricing) — Free/$10/$15 per-seat
- [Cozi Gold](https://www.cozi.com/cozi-gold-features/) — Free/$39/yr per-family
- [FamilyWall Premium](https://www.familywall.com/premium.html) — Free/$5-8/mo per-family

### DACH Payment Market

- [Payments in Germany (Stripe)](https://stripe.com/resources/more/payments-in-germany-an-in-depth-guide)
- [Top Payment Providers DACH](https://ecommercegermany.com/blog/top-20-payment-providers-in-the-dach-region/)
- [Payment Trends 2026 (Wero, A2A)](https://www.xictron.com/en/blog/payment-trends-wero-a2a-2026/)
- [Stripe Alternatives Europe](https://www.european-saas.eu/blog/stripe-alternatives-europe)
