# Payment & Billing — Frontend Architecture

## Route Registration

### app.routes.ts

```typescript
{
  path: 'billing',
  canActivate: [familyMemberGuard],
  loadChildren: () =>
    import('./features/billing/billing.routes').then((m) => m.BILLING_ROUTES),
}
```

### features/billing/billing.routes.ts

```typescript
export const BILLING_ROUTES: Routes = [
  { path: '', component: BillingOverviewPage },
  { path: 'plans', component: PricingPage },
  { path: 'checkout/success', component: CheckoutSuccessPage },
  { path: 'checkout/cancel', component: CheckoutCancelPage },
  { path: 'history', component: BillingHistoryPage },
];
```

### app.config.ts

```typescript
...provideBillingFeature(),
```

---

## Component Architecture

```
features/billing/
  billing.routes.ts
  billing.providers.ts
  components/
    pricing-page/
      pricing-page.component.ts         -- Public plan comparison
      plan-card.component.ts             -- Individual plan card
    billing-overview/
      billing-overview.component.ts      -- Current subscription status
      subscription-status.component.ts   -- Status badge + renewal date
      usage-meters.component.ts          -- Visual usage progress bars
    billing-history/
      billing-history.component.ts       -- Payment list with pagination
      payment-row.component.ts           -- Single payment line item
    checkout-success/
      checkout-success.component.ts      -- Post-checkout confirmation
    checkout-cancel/
      checkout-cancel.component.ts       -- Checkout abandoned
    shared/
      upgrade-prompt.component.ts        -- Reusable upgrade CTA
      feature-gate.directive.ts          -- *featureGate structural directive
  services/
    billing.service.ts                   -- Apollo queries/mutations
    subscription.store.ts                -- Signal-based subscription state
    feature-gate.service.ts              -- Feature access checking
  graphql/
    billing.queries.graphql              -- GetSubscription, GetPlans, GetUsage, GetBillingHistory
    billing.mutations.graphql            -- CreateCheckoutSession, CancelSubscription
```

---

## Key Services

### SubscriptionStore

Signal-based store for the family's subscription state. Fetched once and cached, refreshed after mutations.

```typescript
@Injectable({ providedIn: 'root' })
export class SubscriptionStore {
  private apollo = inject(Apollo);

  subscription = signal<SubscriptionDto | null>(null);
  plans = signal<PlanDto[]>([]);
  usage = signal<UsageSummaryDto | null>(null);
  loading = signal(false);

  async fetchSubscription(): Promise<void> { ... }
  async fetchPlans(): Promise<void> { ... }
  async fetchUsage(): Promise<void> { ... }
}
```

### FeatureGateService

Computed signals for feature access checking. Uses subscription store data.

```typescript
@Injectable({ providedIn: 'root' })
export class FeatureGateService {
  private store = inject(SubscriptionStore);

  currentPlan = computed(() => this.store.subscription()?.plan);
  planTier = computed(() => this.currentPlan()?.tier ?? 'FREE');
  isPremium = computed(() => this.planTier() === 'PREMIUM');

  features = computed(() => this.currentPlan()?.features ?? []);
  limits = computed(() => this.store.usage()?.limits ?? []);

  hasFeature(featureKey: string): Signal<boolean> {
    return computed(() => this.features().includes(featureKey));
  }

  isWithinLimit(limitKey: string): Signal<boolean> {
    return computed(() => {
      const limit = this.limits().find(l => l.key === limitKey);
      if (!limit) return true;
      if (limit.maxValue === -1) return true; // unlimited
      return limit.currentUsage < limit.maxValue;
    });
  }
}
```

### BillingService

Apollo GraphQL operations for billing.

```typescript
@Injectable({ providedIn: 'root' })
export class BillingService {
  private apollo = inject(Apollo);

  getSubscription() { ... }
  getPlans() { ... }
  getUsage() { ... }
  getBillingHistory(first: number, after?: string) { ... }

  createCheckoutSession(planId: string, billingCycle: string, successUrl: string, cancelUrl: string) { ... }
  cancelSubscription(reason?: string) { ... }
  resumeSubscription() { ... }
  changeBillingCycle(billingCycle: string) { ... }

  getCustomerPortalUrl(returnUrl: string) { ... }
}
```

---

## Checkout Flow (Stripe Checkout — Hosted)

For Phase 1, use Stripe Checkout (hosted payment page) rather than a custom form.

```
1. User clicks "Upgrade to Premium" on pricing page
2. Frontend calls CreateCheckoutSession mutation (planId + billingCycle)
3. Backend creates Stripe Checkout Session, returns checkoutUrl
4. Frontend redirects: window.location.href = checkoutUrl
5. User completes payment on Stripe's hosted page (SEPA, Card, PayPal)
6. Stripe redirects to /billing/checkout/success?session_id={SESSION_ID}
7. Backend receives webhook: checkout.session.completed
8. Backend creates Subscription + Payment records
9. CheckoutSuccessPage fetches updated subscription via GraphQL
```

### Why Stripe Checkout (not embedded Elements)

- Zero PCI burden (SAQ-A)
- Automatic German localization (DE locale)
- Built-in SEPA mandate collection
- Built-in 3D Secure / SCA handling
- Mobile-optimized
- Saves 2-3 weeks of frontend development

---

## Billing Management via Stripe Customer Portal

For managing payment methods, downloading invoices, and updating billing info:

```typescript
// billing-overview.component.ts
async openCustomerPortal() {
  const result = await this.billingService.getCustomerPortalUrl('/billing');
  window.location.href = result;
}
```

Stripe Customer Portal handles:

- Payment method updates (add/remove cards, SEPA mandates)
- Invoice downloads (PDF, tax-compliant)
- Subscription cancellation confirmation
- Fully localized for German

---

## Feature Gate Directive

Structural directive for conditionally showing/hiding premium features throughout the app.

```typescript
@Directive({
  selector: '[featureGate]',
  standalone: true,
})
export class FeatureGateDirective implements OnInit {
  @Input() featureGate!: string;
  @Input() featureGateElse?: TemplateRef<unknown>;

  private featureGateService = inject(FeatureGateService);
  private templateRef = inject(TemplateRef);
  private viewContainer = inject(ViewContainerRef);

  ngOnInit() {
    // Effect: show content if feature available, else show upgrade prompt
    effect(() => {
      const hasFeature = this.featureGateService.hasFeature(this.featureGate)();
      this.viewContainer.clear();
      if (hasFeature) {
        this.viewContainer.createEmbeddedView(this.templateRef);
      } else if (this.featureGateElse) {
        this.viewContainer.createEmbeddedView(this.featureGateElse);
      }
    });
  }
}
```

### Usage

```html
<div *featureGate="'advanced_event_chains'; else upgradePrompt">
  <!-- Advanced event chain builder content -->
</div>
<ng-template #upgradePrompt>
  <app-upgrade-prompt feature="Advanced Event Chains" />
</ng-template>
```

---

## Upgrade Prompt Component

Reusable CTA shown when a premium feature is gated.

```typescript
@Component({
  selector: 'app-upgrade-prompt',
  standalone: true,
  imports: [RouterModule],
  template: `
    <div class="rounded-lg border border-amber-200 bg-amber-50 p-4">
      <p class="text-sm text-amber-800">
        {{ feature() }} requires <strong>Family Pro</strong>
      </p>
      <a routerLink="/billing/plans"
         class="mt-2 inline-block text-sm font-medium text-amber-700 underline">
        View plans
      </a>
    </div>
  `,
})
export class UpgradePromptComponent {
  feature = input.required<string>();
}
```

---

## Pricing Page

Visual plan comparison with CTA buttons.

```
+-------------------------------+-------------------------------+
|     Family Essentials         |       Family Pro              |
|          FREE                 |   6.99 EUR/mo | 59.99 EUR/yr |
|                               |                               |
|  5 family members             |  15 family members            |
|  5 event chains               |  Unlimited event chains       |
|  50 automations/month         |  Unlimited automations        |
|  100 MB storage               |  5 GB storage                 |
|                               |  Premium widgets              |
|                               |  Advanced event chain builder |
|                               |  Priority support             |
|                               |                               |
|  [Current Plan]               |  [Start 14-Day Free Trial]   |
+-------------------------------+-------------------------------+
```

- Monthly/Yearly toggle switch
- Annual savings badge: "Save 28%"
- Current plan highlighted
- CTA: "Start 14-Day Free Trial" (or "Upgrade" if trial already used)

---

## Usage Meters (Billing Overview)

Visual progress bars showing current usage vs limits.

```
Family Members        [=====     ] 3 / 5
Event Chains          [========  ] 4 / 5
Automations (month)   [===       ] 15 / 50
Storage               [==        ] 23 MB / 100 MB
```

- Green: < 75% used
- Amber: 75-90% used
- Red: > 90% used
- Shows "Unlimited" for Premium features with -1 limit
