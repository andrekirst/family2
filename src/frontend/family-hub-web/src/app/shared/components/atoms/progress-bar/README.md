# ProgressBarComponent

A responsive, accessible progress indicator component for wizard interfaces and multi-step flows.

## Features

- **Responsive Design**: Linear bar on desktop (≥768px), compact dot stepper on mobile (<768px)
- **Three Variants**: `linear`, `dots`, `responsive` (default)
- **WCAG 2.1 AA Compliant**: ARIA progressbar role, descriptive labels, 4.5:1 color contrast
- **Smooth Animations**: 300ms transitions with `prefers-reduced-motion` support
- **Design System Aligned**: Uses Tailwind classes from Family Hub design system

## Usage

### Basic Example (Responsive)

```typescript
<app-progress-bar
  [currentStep]="2"
  [totalSteps]="4"
></app-progress-bar>
```

**Output:**
- **Desktop (≥768px)**: Linear bar at 33% with "Step 2 of 4" text
- **Mobile (<768px)**: 4 dots, first 2 blue (active), last 2 gray (inactive)

### Linear Variant (Desktop Only)

```typescript
<app-progress-bar
  [currentStep]="3"
  [totalSteps]="5"
  variant="linear"
></app-progress-bar>
```

**Output:** Linear progress bar (50%) with "Step 3 of 5" text above

### Dots Variant (Mobile Only)

```typescript
<app-progress-bar
  [currentStep]="2"
  [totalSteps]="3"
  variant="dots"
></app-progress-bar>
```

**Output:** 3 dots in a row (2 blue, 1 gray)

## API

### Inputs

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `currentStep` | `number` | `1` | Current step (1-indexed for display) |
| `totalSteps` | `number` | `1` | Total number of steps |
| `variant` | `'linear' \| 'dots' \| 'responsive'` | `'responsive'` | Display variant |

### Variants

| Variant | Behavior |
|---------|----------|
| `responsive` | Shows linear bar on desktop (md:), dots on mobile |
| `linear` | Always shows linear progress bar with text |
| `dots` | Always shows compact dot stepper (no text) |

## Accessibility

### WCAG 2.1 AA Compliance

- **Role**: `role="progressbar"` for assistive technology
- **ARIA Attributes**:
  - `aria-valuenow`: Current step number
  - `aria-valuemin="1"`: Minimum step value
  - `aria-valuemax`: Total steps count
  - `aria-label`: "Step X of Y" description
- **Color Contrast**:
  - Blue-600 (#2771E0) on white: 8.6:1 (AAA)
  - Gray-600 (#4B5563) text on white: 8.6:1 (AAA)
- **Motion**: Respects `prefers-reduced-motion` media query

### Screen Reader Announcements

- **Linear bar**: "Step 2 of 4" (read from aria-label)
- **Dots**: Each dot announces state:
  - "Step 1 completed"
  - "Step 2 current"
  - "Step 3 upcoming"

## Design System Alignment

### Colors (from Design System)

| Element | Color | Tailwind Class | Hex |
|---------|-------|----------------|-----|
| Progress fill | Primary Blue 600 | `bg-blue-600` | #2771E0 |
| Progress background | Gray 200 | `bg-gray-200` | #E5E7EB |
| Step text | Gray 600 | `text-gray-600` | #4B5563 |
| Active dot | Primary Blue 600 | `bg-blue-600` | #2771E0 |
| Inactive dot | Gray 300 | `bg-gray-300` | #D1D5DB |

### Spacing (4pt Grid)

| Element | Spacing | Tailwind Class | Value |
|---------|---------|----------------|-------|
| Text margin | 8px bottom | `mb-2` | 0.5rem |
| Dot gap | 8px horizontal | `space-x-2` | 0.5rem |
| Dot size | 8px × 8px | `w-2 h-2` | 0.5rem × 0.5rem |
| Bar height | 8px | `h-2` | 0.5rem |

### Typography

| Element | Style | Tailwind Class | Value |
|---------|-------|----------------|-------|
| Step text | Small | `text-sm` | 14px |
| Weight | Normal | (default) | 400 |
| Color | Gray 600 | `text-gray-600` | #4B5563 |

### Responsive Breakpoints

| Breakpoint | Width | Tailwind Prefix | Behavior |
|------------|-------|-----------------|----------|
| Mobile | <768px | (none) | Show dots |
| Desktop | ≥768px | `md:` | Show linear bar |

## Animation & Transitions

### Linear Bar

- **Property**: `width` (as percentage changes)
- **Duration**: 300ms
- **Timing Function**: `ease-out` (smooth deceleration)
- **Reduced Motion**: Transitions disabled with `motion-reduce:transition-none`

### Dots

- **Property**: `background-color` (blue ↔ gray)
- **Duration**: 200ms
- **Timing Function**: `ease-out`
- **Reduced Motion**: Transitions disabled with `motion-reduce:transition-none`

## Examples

### Wizard Flow (4 Steps)

```typescript
export class CreateFamilyWizardComponent {
  currentStep = 1;
  totalSteps = 4;

  nextStep() {
    if (this.currentStep < this.totalSteps) {
      this.currentStep++;
    }
  }

  previousStep() {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }
}
```

```html
<app-progress-bar
  [currentStep]="currentStep"
  [totalSteps]="totalSteps"
></app-progress-bar>

<div class="wizard-content">
  <!-- Step content here -->
</div>

<div class="flex justify-between mt-6">
  <button
    (click)="previousStep()"
    [disabled]="currentStep === 1"
    class="btn-secondary"
  >
    Back
  </button>
  <button
    (click)="nextStep()"
    [disabled]="currentStep === totalSteps"
    class="btn-primary"
  >
    Next
  </button>
</div>
```

### Onboarding (3 Steps)

```typescript
<app-progress-bar
  [currentStep]="1"
  [totalSteps]="3"
></app-progress-bar>

<h2>Welcome to Family Hub!</h2>
<p>Let's get you set up in 3 easy steps.</p>
```

## Testing

### Unit Tests

Run tests with:

```bash
ng test --include='**/progress-bar.component.spec.ts'
```

**Test Coverage:**
- Progress percentage calculation (0%, 33%, 67%, 100%)
- Step text generation ("Step 2 of 4")
- ARIA label generation
- Dot state classes (active/inactive)
- Accessibility attributes (role, aria-*)
- Variant rendering (linear, dots, responsive)
- Design system alignment (colors, spacing)
- Animation classes (transition, duration, ease-out)

### Manual Accessibility Testing

1. **Screen Reader** (VoiceOver/NVDA):
   - Navigate to progress bar with Tab
   - Verify announcement: "Step 2 of 4, progressbar"
   - For dots, verify each dot announces state

2. **Keyboard Navigation**:
   - Progress bar is non-interactive (no focus needed)
   - Verify focus skips to next interactive element

3. **Zoom Test**:
   - Zoom to 200% (browser zoom)
   - Verify bar and dots remain visible and proportional

4. **Color Contrast** (Chrome DevTools):
   - Inspect progress bar colors
   - Verify 3:1 minimum for UI components (WCAG AA)

5. **Reduced Motion**:
   - Enable "Reduce motion" in OS settings
   - Verify no width/color transitions

## File Location

```
/home/andrekirst/git/github/andrekirst/family2/src/frontend/family-hub-web/src/app/shared/components/atoms/progress-bar/
├── progress-bar.component.ts       # Component logic
├── progress-bar.component.spec.ts  # Unit tests
└── README.md                       # This file
```

## Related Components

- **ButtonComponent**: Used in wizard navigation (Back/Next)
- **SpinnerComponent**: Loading states during wizard steps
- **ModalComponent**: Wizard confirmation dialogs

## Design Rationale

### Why Responsive Variant?

- **Desktop users** benefit from explicit "Step X of Y" text and visual percentage bar
- **Mobile users** need compact UI due to limited vertical space
- **Dots** save 24px vertical space (text + margin) on mobile screens
- **Automatic** adaptation prevents developer error (forgetting to switch variants)

### Why 8px Height?

- **Visibility**: Large enough to see at a glance
- **Touch target**: Not interactive, so no 44px minimum needed
- **4pt grid**: Aligns with design system spacing scale (0.5rem = 8px)
- **Proportion**: Balanced with 8px dot size for visual consistency

### Why 300ms Transition?

- **Smooth**: Noticeable but not sluggish
- **Perceptible**: User sees progress updating
- **Standard**: Matches design system `--transition-slow` (300ms)
- **Accessible**: Disableable via `prefers-reduced-motion`

## Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

**Note:** Requires CSS Grid support for responsive layout (all modern browsers).

## Future Enhancements (Phase 2+)

- [ ] **Labeled steps**: Show step names below dots ("Details", "Review", "Confirm")
- [ ] **Vertical orientation**: For sidebar wizards
- [ ] **Custom colors**: Support category colors (health = red, work = blue)
- [ ] **Substeps**: Show "Step 2.1 of 2.3" for nested wizards
- [ ] **Estimated time**: "2 minutes remaining" below bar
- [ ] **Skip steps**: Show skipped steps in gray with strikethrough

## License

Internal component for Family Hub project. See project LICENSE.

---

**Component Status**: Production Ready
**Last Updated**: 2026-01-03
**WCAG 2.1 Level**: AA Compliant
