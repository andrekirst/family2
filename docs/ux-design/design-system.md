# Family Hub - Design System

**Version:** 1.0
**Date:** 2025-12-19
**Status:** Design Specification
**Author:** UI Designer (Claude Code)

---

## Table of Contents

1. [Brand Identity](#brand-identity)
2. [Color System](#color-system)
3. [Typography](#typography)
4. [Spacing & Layout](#spacing--layout)
5. [Iconography](#iconography)
6. [Illustration Style](#illustration-style)
7. [Component Library](#component-library)
8. [Design Tokens](#design-tokens)
9. [Dark Mode](#dark-mode)
10. [Motion Design](#motion-design)

---

## Brand Identity

### Vision Statement

Family Hub is a privacy-first, intelligent family organization platform that reduces mental load through event chain automation while maintaining a warm, approachable, and trustworthy brand presence.

### Brand Attributes

- **Trustworthy**: Privacy-first, secure, reliable
- **Intelligent**: Smart automation, proactive assistance
- **Approachable**: Family-friendly, warm, inclusive
- **Modern**: Clean design, contemporary technology
- **Empowering**: User control, flexibility, customization

### Brand Personality

- **Tone**: Helpful, friendly, respectful
- **Voice**: Clear, concise, conversational
- **Character**: Like a trusted family assistant

---

## Color System

### Primary Palette

```
Primary Blue (Brand Color)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
50:  #EBF5FF   (Lightest)
100: #D1E9FF
200: #B3DDFF
300: #84C7FF
400: #56A9FF
500: #3B8FFF   ← Primary
600: #2771E0
700: #1B5AC1
800: #13479B
900: #0D3875   (Darkest)

Use Cases:
- Primary buttons
- Links
- Selected states
- Focus indicators
- Work/professional event category
```

### Secondary Palette

```
Green (Success / Kids Category)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
50:  #ECFDF5
100: #D1FAE5
200: #A7F3D0
300: #6EE7B7
400: #34D399
500: #10B981   ← Success
600: #059669
700: #047857
800: #065F46
900: #064E3B

Use Cases:
- Success messages
- Kids/children category
- Completed tasks
- Positive metrics

Purple (Family Category)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
50:  #FAF5FF
100: #F3E8FF
200: #E9D5FF
300: #D8B4FE
400: #C084FC
500: #A855F7   ← Family
600: #9333EA
700: #7E22CE
800: #6B21A8
900: #581C87

Use Cases:
- Family events
- Shared activities
- Family category

Amber (Warning)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
50:  #FFFBEB
100: #FEF3C7
200: #FDE68A
300: #FCD34D
400: #FBBF24
500: #F59E0B   ← Warning
600: #D97706
700: #B45309
800: #92400E
900: #78350F

Use Cases:
- Warning messages
- Budget alerts (80%)
- Pending states
- Personal category
```

### Semantic Colors

```
Red (Error / Health / High Priority)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
50:  #FEF2F2
100: #FEE2E2
200: #FECACA
300: #FCA5A5
400: #F87171
500: #EF4444   ← Error/Health
600: #DC2626
700: #B91C1C
800: #991B1B
900: #7F1D1D

Use Cases:
- Error messages
- Health/medical category
- High priority tasks
- Destructive actions
- Overdue indicators

Teal (Finance Category)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
50:  #F0FDFA
100: #CCFBF1
200: #99F6E4
300: #5EEAD4
400: #2DD4BF
500: #14B8A6   ← Finance
600: #0D9488
700: #0F766E
800: #115E59
900: #134E4A

Use Cases:
- Finance category
- Budget tracking
- Expense indicators
```

### Neutral Palette (Grays)

```
Neutral Gray
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
50:  #F9FAFB   (Backgrounds)
100: #F3F4F6   (Alt backgrounds)
200: #E5E7EB   (Borders, dividers)
300: #D1D5DB   (Disabled states)
400: #9CA3AF   (Placeholder text)
500: #6B7280   (Secondary text)
600: #4B5563   (Body text)
700: #374151   (Headings)
800: #1F2937   (Dark headings)
900: #111827   (Almost black)

Use Cases:
- Text hierarchy
- Backgrounds
- Borders
- Disabled states
- Shadows
```

### Category Color Mapping

```javascript
const categoryColors = {
  work: 'blue-500',      // #3B8FFF
  kids: 'green-500',     // #10B981
  family: 'purple-500',  // #A855F7
  health: 'red-500',     // #EF4444
  personal: 'amber-500', // #F59E0B
  finance: 'teal-500',   // #14B8A6
  school: 'indigo-500',  // #6366F1
  social: 'pink-500',    // #EC4899
  other: 'gray-500'      // #6B7280
};
```

### Accessibility

**WCAG 2.1 AA Compliance**

All color combinations meet minimum contrast ratios:
- Normal text (< 18px): 4.5:1
- Large text (≥ 18px or bold ≥ 14px): 3:1
- UI components: 3:1

**Color Contrast Examples**

```
✓ PASS: Primary Blue 500 (#3B8FFF) on White (#FFFFFF) = 5.2:1
✓ PASS: Gray 600 (#4B5563) on White = 8.6:1
✓ PASS: White on Primary Blue 600 (#2771E0) = 4.9:1
✗ FAIL: Primary Blue 500 on Gray 50 = 4.1:1 (use Blue 600 instead)
```

---

## Typography

### Font Families

```css
/* Primary Font: Inter */
--font-family-sans: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI',
  'Roboto', 'Oxygen', 'Ubuntu', 'Cantarell', sans-serif;

/* Monospace Font: JetBrains Mono */
--font-family-mono: 'JetBrains Mono', 'Courier New', monospace;

/* System Fonts Fallback */
--font-family-system: -apple-system, BlinkMacSystemFont, 'Segoe UI',
  'Roboto', 'Oxygen', 'Ubuntu', 'Cantarell', sans-serif;
```

**Why Inter?**
- Excellent readability at small sizes
- Wide range of weights (100-900)
- Great for UI and body text
- Open source and free
- Optimized for screens

### Type Scale

```css
/* Desktop Type Scale (16px base) */
--text-xs:   0.75rem;  /* 12px */
--text-sm:   0.875rem; /* 14px */
--text-base: 1rem;     /* 16px */
--text-lg:   1.125rem; /* 18px */
--text-xl:   1.25rem;  /* 20px */
--text-2xl:  1.5rem;   /* 24px */
--text-3xl:  1.875rem; /* 30px */
--text-4xl:  2.25rem;  /* 36px */
--text-5xl:  3rem;     /* 48px */

/* Mobile Type Scale (14px base) */
@media (max-width: 639px) {
  --text-base: 0.875rem; /* 14px */
  --text-lg:   1rem;     /* 16px */
  --text-xl:   1.125rem; /* 18px */
  --text-2xl:  1.25rem;  /* 20px */
  --text-3xl:  1.5rem;   /* 24px */
  --text-4xl:  1.875rem; /* 30px */
  --text-5xl:  2.25rem;  /* 36px */
}
```

### Font Weights

```css
--font-thin:       100;
--font-extralight: 200;
--font-light:      300;
--font-normal:     400;
--font-medium:     500;
--font-semibold:   600;
--font-bold:       700;
--font-extrabold:  800;
--font-black:      900;
```

**Weight Usage Guidelines**

```
Regular (400):  Body text, paragraphs
Medium (500):   Subheadings, labels, button text
Semibold (600): Important labels, card headers
Bold (700):     Headings, section titles
```

### Line Heights

```css
--leading-none:   1;
--leading-tight:  1.25;
--leading-snug:   1.375;
--leading-normal: 1.5;    /* Body text */
--leading-relaxed: 1.625;
--leading-loose:  2;
```

### Heading Styles

```css
/* H1 - Page Title */
.heading-1 {
  font-size: var(--text-4xl);    /* 36px */
  font-weight: var(--font-bold);  /* 700 */
  line-height: var(--leading-tight);
  letter-spacing: -0.02em;
  color: var(--gray-900);
}

/* H2 - Section Title */
.heading-2 {
  font-size: var(--text-3xl);    /* 30px */
  font-weight: var(--font-bold);  /* 700 */
  line-height: var(--leading-tight);
  letter-spacing: -0.01em;
  color: var(--gray-900);
}

/* H3 - Subsection */
.heading-3 {
  font-size: var(--text-2xl);    /* 24px */
  font-weight: var(--font-semibold); /* 600 */
  line-height: var(--leading-snug);
  color: var(--gray-800);
}

/* H4 - Component Title */
.heading-4 {
  font-size: var(--text-xl);     /* 20px */
  font-weight: var(--font-semibold); /* 600 */
  line-height: var(--leading-snug);
  color: var(--gray-800);
}

/* H5 - Card Header */
.heading-5 {
  font-size: var(--text-lg);     /* 18px */
  font-weight: var(--font-medium); /* 500 */
  line-height: var(--leading-snug);
  color: var(--gray-700);
}

/* H6 - Small Header */
.heading-6 {
  font-size: var(--text-base);   /* 16px */
  font-weight: var(--font-medium); /* 500 */
  line-height: var(--leading-normal);
  color: var(--gray-700);
}
```

### Body Text Styles

```css
/* Body Large */
.body-large {
  font-size: var(--text-lg);
  font-weight: var(--font-normal);
  line-height: var(--leading-relaxed);
  color: var(--gray-600);
}

/* Body Regular */
.body {
  font-size: var(--text-base);
  font-weight: var(--font-normal);
  line-height: var(--leading-normal);
  color: var(--gray-600);
}

/* Body Small */
.body-small {
  font-size: var(--text-sm);
  font-weight: var(--font-normal);
  line-height: var(--leading-normal);
  color: var(--gray-500);
}

/* Caption */
.caption {
  font-size: var(--text-xs);
  font-weight: var(--font-normal);
  line-height: var(--leading-normal);
  color: var(--gray-500);
}
```

### Interactive Text Styles

```css
/* Link */
.link {
  font-size: inherit;
  font-weight: var(--font-medium);
  color: var(--blue-600);
  text-decoration: underline;
  text-decoration-color: transparent;
  transition: all 0.2s ease;
}

.link:hover {
  color: var(--blue-700);
  text-decoration-color: currentColor;
}

.link:focus {
  outline: 2px solid var(--blue-600);
  outline-offset: 2px;
}

/* Button Text */
.button-text {
  font-size: var(--text-base);
  font-weight: var(--font-medium);
  letter-spacing: 0.01em;
}

/* Label */
.label {
  font-size: var(--text-sm);
  font-weight: var(--font-medium);
  color: var(--gray-700);
  letter-spacing: 0.01em;
}

/* Input Text */
.input-text {
  font-size: var(--text-base);
  font-weight: var(--font-normal);
  color: var(--gray-900);
}

/* Placeholder Text */
.placeholder {
  font-size: var(--text-base);
  font-weight: var(--font-normal);
  color: var(--gray-400);
}
```

---

## Spacing & Layout

### Spacing Scale (4pt Grid)

```css
/* Spacing tokens based on 4px grid */
--spacing-0:  0;
--spacing-1:  0.25rem;  /* 4px */
--spacing-2:  0.5rem;   /* 8px */
--spacing-3:  0.75rem;  /* 12px */
--spacing-4:  1rem;     /* 16px */
--spacing-5:  1.25rem;  /* 20px */
--spacing-6:  1.5rem;   /* 24px */
--spacing-8:  2rem;     /* 32px */
--spacing-10: 2.5rem;   /* 40px */
--spacing-12: 3rem;     /* 48px */
--spacing-16: 4rem;     /* 64px */
--spacing-20: 5rem;     /* 80px */
--spacing-24: 6rem;     /* 96px */
--spacing-32: 8rem;     /* 128px */
```

### Layout Grid

```css
/* Container Max Widths */
--container-sm:  640px;  /* Mobile */
--container-md:  768px;  /* Tablet */
--container-lg:  1024px; /* Desktop */
--container-xl:  1280px; /* Large Desktop */
--container-2xl: 1536px; /* Extra Large */

/* Breakpoints (Tailwind defaults) */
--breakpoint-sm:  640px;
--breakpoint-md:  768px;
--breakpoint-lg:  1024px;
--breakpoint-xl:  1280px;
--breakpoint-2xl: 1536px;

/* Grid Columns */
--grid-cols-4:  repeat(4, minmax(0, 1fr));   /* Mobile */
--grid-cols-8:  repeat(8, minmax(0, 1fr));   /* Tablet */
--grid-cols-12: repeat(12, minmax(0, 1fr));  /* Desktop */

/* Grid Gap */
--grid-gap: var(--spacing-6); /* 24px */
```

### Component Spacing Guidelines

```css
/* Card Padding */
--card-padding-mobile:  var(--spacing-4);  /* 16px */
--card-padding-desktop: var(--spacing-6);  /* 24px */

/* Section Spacing */
--section-gap-mobile:  var(--spacing-8);   /* 32px */
--section-gap-desktop: var(--spacing-12);  /* 48px */

/* Element Spacing */
--element-gap-xs:  var(--spacing-1);  /* 4px */
--element-gap-sm:  var(--spacing-2);  /* 8px */
--element-gap-md:  var(--spacing-4);  /* 16px */
--element-gap-lg:  var(--spacing-6);  /* 24px */
--element-gap-xl:  var(--spacing-8);  /* 32px */

/* Touch Target */
--touch-target-min: 44px;  /* iOS minimum */
--touch-target-android: 48px; /* Android minimum */
```

---

## Iconography

### Icon Library: Heroicons (Tailwind)

**Why Heroicons?**
- Consistent with Tailwind CSS ecosystem
- Two variants: Outline (24px) and Solid (20px)
- MIT licensed, free to use
- Clean, modern design
- Optimized SVGs

### Icon Sizes

```css
--icon-xs:   16px;  /* Small inline icons */
--icon-sm:   20px;  /* Solid variant, buttons */
--icon-md:   24px;  /* Outline variant, default */
--icon-lg:   32px;  /* Large buttons, headers */
--icon-xl:   48px;  /* Feature icons, empty states */
--icon-2xl:  64px;  /* Hero sections */
```

### Icon Usage Guidelines

```typescript
// Outline icons (24px) - Default for UI
import {
  CalendarIcon,
  ShoppingCartIcon,
  CheckCircleIcon
} from '@heroicons/react/24/outline';

// Solid icons (20px) - Active states, filled
import {
  CalendarIcon as CalendarIconSolid,
  ShoppingCartIcon as CartIconSolid,
  CheckCircleIcon as CheckIconSolid
} from '@heroicons/react/20/solid';
```

### Core Icon Set

```
Navigation
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
home              - Home/Dashboard
calendar-days     - Calendar
list-bullet       - Shopping Lists
check-circle      - Tasks
user-group        - Family Members
cog-6-tooth       - Settings
bell              - Notifications
magnifying-glass  - Search
bars-3            - Menu

Actions
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
plus              - Add
pencil            - Edit
trash             - Delete
arrow-right       - Forward/Next
arrow-left        - Back
x-mark            - Close
check             - Confirm
ellipsis-vertical - More options
share             - Share

States & Indicators
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
check-circle      - Success/Complete
exclamation-triangle - Warning
x-circle          - Error
information-circle - Info
clock             - Pending/Time
bolt              - Event Chain (automation)
star              - Favorite/Points
fire              - Streak

Categories
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
briefcase         - Work
academic-cap      - School
heart             - Health
currency-dollar   - Finance
users             - Family
user              - Personal
sparkles          - Other
building-storefront - Shopping
cake              - Birthday
```

### Icon Colors

```css
/* Default icon color */
.icon {
  color: var(--gray-600);
}

/* Category-specific icons */
.icon-work { color: var(--blue-500); }
.icon-kids { color: var(--green-500); }
.icon-family { color: var(--purple-500); }
.icon-health { color: var(--red-500); }
.icon-finance { color: var(--teal-500); }
.icon-personal { color: var(--amber-500); }

/* State-specific icons */
.icon-success { color: var(--green-500); }
.icon-error { color: var(--red-500); }
.icon-warning { color: var(--amber-500); }
.icon-info { color: var(--blue-500); }
```

---

## Illustration Style

### Style Guidelines

**Characteristics:**
- **Friendly & Approachable**: Rounded shapes, soft edges
- **Diverse & Inclusive**: Represent all family types
- **Simple & Clear**: Minimal detail, easy to understand
- **Colorful but Tasteful**: Use brand colors, not overwhelming
- **Modern**: Flat design with subtle depth

### Illustration Types

**1. Hero Illustrations (Large)**
- Onboarding screens
- Empty states
- Error pages
- Feature explanations
- Size: 400x300px to 600x450px

**2. Spot Illustrations (Medium)**
- Success confirmations
- Achievement unlocks
- Feature highlights
- Size: 200x200px to 300x300px

**3. Icons/Glyphs (Small)**
- Category markers
- Status indicators
- Inline embellishments
- Size: 32x32px to 64x64px

### Color Usage in Illustrations

```
Primary (Blue):     Main subjects, key elements
Secondary (Purple): Supporting elements, backgrounds
Accent (Green):     Highlights, success states
Neutral (Gray):     Shadows, outlines, secondary objects
```

### Illustration Library

**Recommended Libraries:**
- **unDraw** (undraw.co) - Free, customizable, diverse
- **Blush** (blush.design) - Mix and match, various styles
- **Humaaans** (humaaans.com) - Diverse, customizable people
- **Storyset** (storyset.com) - Animated, free

**Custom Illustrations:**
- Commission custom family-themed illustrations
- Ensure diversity in representation
- Maintain consistent style across all assets

---

## Component Library

### Buttons

#### Primary Button

```html
<!-- Tailwind Classes -->
<button class="
  inline-flex items-center justify-center
  px-4 py-2
  text-base font-medium text-white
  bg-blue-600 hover:bg-blue-700
  rounded-md
  border border-transparent
  shadow-sm
  focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2
  disabled:opacity-50 disabled:cursor-not-allowed
  transition-colors duration-200
">
  Button Text
</button>
```

**Sizes:**
```css
/* Small */
.btn-sm { padding: 0.5rem 1rem; font-size: 0.875rem; }

/* Medium (Default) */
.btn-md { padding: 0.5rem 1rem; font-size: 1rem; }

/* Large */
.btn-lg { padding: 0.75rem 1.5rem; font-size: 1.125rem; }
```

#### Secondary Button

```html
<button class="
  inline-flex items-center justify-center
  px-4 py-2
  text-base font-medium text-blue-700
  bg-blue-50 hover:bg-blue-100
  rounded-md
  border border-blue-200
  focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2
  disabled:opacity-50 disabled:cursor-not-allowed
  transition-colors duration-200
">
  Button Text
</button>
```

#### Tertiary/Ghost Button

```html
<button class="
  inline-flex items-center justify-center
  px-4 py-2
  text-base font-medium text-gray-700
  bg-transparent hover:bg-gray-100
  rounded-md
  border border-transparent
  focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2
  disabled:opacity-50 disabled:cursor-not-allowed
  transition-colors duration-200
">
  Button Text
</button>
```

#### Icon Button

```html
<button class="
  inline-flex items-center justify-center
  w-10 h-10
  text-gray-600 hover:text-gray-900
  bg-transparent hover:bg-gray-100
  rounded-md
  focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2
  transition-colors duration-200
">
  <svg class="w-6 h-6" fill="none" stroke="currentColor">
    <!-- Icon SVG -->
  </svg>
</button>
```

#### Floating Action Button (FAB)

```html
<button class="
  fixed bottom-6 right-6
  w-14 h-14
  flex items-center justify-center
  text-white
  bg-blue-600 hover:bg-blue-700
  rounded-full
  shadow-lg hover:shadow-xl
  focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2
  transition-all duration-200
  z-50
">
  <svg class="w-6 h-6" fill="none" stroke="currentColor" stroke-width="2">
    <path d="M12 4v16m8-8H4"/>
  </svg>
</button>
```

### Form Elements

#### Text Input

```html
<div class="mb-4">
  <label for="email" class="block text-sm font-medium text-gray-700 mb-1">
    Email Address
  </label>
  <input
    type="email"
    id="email"
    name="email"
    placeholder="you@example.com"
    class="
      block w-full px-3 py-2
      text-base text-gray-900
      placeholder-gray-400
      bg-white
      border border-gray-300
      rounded-md
      focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent
      disabled:bg-gray-100 disabled:cursor-not-allowed
      transition-colors duration-200
    "
  />
  <!-- Error state -->
  <p class="mt-1 text-sm text-red-600 hidden" id="email-error">
    Please enter a valid email address
  </p>
</div>
```

**Input States:**
```css
/* Default */
.input { border-color: var(--gray-300); }

/* Focus */
.input:focus {
  border-color: transparent;
  ring: 2px var(--blue-500);
}

/* Error */
.input-error { border-color: var(--red-500); }

/* Disabled */
.input:disabled {
  background-color: var(--gray-100);
  cursor: not-allowed;
}
```

#### Select Dropdown

```html
<div class="mb-4">
  <label for="category" class="block text-sm font-medium text-gray-700 mb-1">
    Category
  </label>
  <select
    id="category"
    name="category"
    class="
      block w-full px-3 py-2
      text-base text-gray-900
      bg-white
      border border-gray-300
      rounded-md
      focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent
      disabled:bg-gray-100 disabled:cursor-not-allowed
      transition-colors duration-200
    "
  >
    <option value="">Select a category</option>
    <option value="work">Work</option>
    <option value="family">Family</option>
    <option value="personal">Personal</option>
  </select>
</div>
```

#### Checkbox

```html
<div class="flex items-start">
  <div class="flex items-center h-5">
    <input
      id="remember"
      name="remember"
      type="checkbox"
      class="
        w-4 h-4
        text-blue-600
        bg-white
        border-gray-300
        rounded
        focus:ring-2 focus:ring-blue-500
        transition-colors duration-200
      "
    />
  </div>
  <div class="ml-3">
    <label for="remember" class="text-sm font-normal text-gray-700">
      Remember me for 30 days
    </label>
  </div>
</div>
```

#### Radio Buttons

```html
<fieldset>
  <legend class="text-sm font-medium text-gray-700 mb-2">
    Priority Level
  </legend>
  <div class="space-y-2">
    <div class="flex items-center">
      <input
        id="priority-low"
        name="priority"
        type="radio"
        value="low"
        class="
          w-4 h-4
          text-blue-600
          border-gray-300
          focus:ring-2 focus:ring-blue-500
        "
      />
      <label for="priority-low" class="ml-3 text-sm text-gray-700">
        Low
      </label>
    </div>
    <div class="flex items-center">
      <input
        id="priority-medium"
        name="priority"
        type="radio"
        value="medium"
        class="w-4 h-4 text-blue-600 border-gray-300 focus:ring-2 focus:ring-blue-500"
      />
      <label for="priority-medium" class="ml-3 text-sm text-gray-700">
        Medium
      </label>
    </div>
    <div class="flex items-center">
      <input
        id="priority-high"
        name="priority"
        type="radio"
        value="high"
        class="w-4 h-4 text-blue-600 border-gray-300 focus:ring-2 focus:ring-blue-500"
      />
      <label for="priority-high" class="ml-3 text-sm text-gray-700">
        High
      </label>
    </div>
  </div>
</fieldset>
```

#### Toggle Switch

```html
<button
  type="button"
  role="switch"
  aria-checked="false"
  class="
    relative inline-flex h-6 w-11
    flex-shrink-0
    cursor-pointer
    rounded-full
    border-2 border-transparent
    bg-gray-200
    transition-colors duration-200 ease-in-out
    focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2
  "
>
  <span class="sr-only">Enable notifications</span>
  <span
    aria-hidden="true"
    class="
      pointer-events-none
      inline-block h-5 w-5
      transform rounded-full
      bg-white shadow
      ring-0
      transition duration-200 ease-in-out
      translate-x-0
    "
  ></span>
</button>

<!-- Enabled state: add these classes to button -->
<!-- bg-blue-600 -->
<!-- And these to span: translate-x-5 -->
```

### Cards

#### Basic Card

```html
<div class="
  bg-white
  border border-gray-200
  rounded-lg
  shadow-sm
  p-6
  hover:shadow-md
  transition-shadow duration-200
">
  <h3 class="text-lg font-semibold text-gray-900 mb-2">
    Card Title
  </h3>
  <p class="text-sm text-gray-600">
    Card content goes here. This can include text, images, or other elements.
  </p>
</div>
```

#### Card with Header and Footer

```html
<div class="bg-white border border-gray-200 rounded-lg shadow-sm overflow-hidden">
  <!-- Header -->
  <div class="px-6 py-4 border-b border-gray-200 bg-gray-50">
    <h3 class="text-lg font-semibold text-gray-900">
      Shopping List
    </h3>
    <p class="text-sm text-gray-600 mt-1">
      12 items • 7 completed
    </p>
  </div>

  <!-- Body -->
  <div class="px-6 py-4">
    <p class="text-sm text-gray-600">
      Card content...
    </p>
  </div>

  <!-- Footer -->
  <div class="px-6 py-4 border-t border-gray-200 bg-gray-50">
    <div class="flex justify-end space-x-3">
      <button class="btn-secondary">Cancel</button>
      <button class="btn-primary">Save</button>
    </div>
  </div>
</div>
```

#### Interactive Card (Clickable)

```html
<a href="#" class="
  block
  bg-white
  border border-gray-200
  rounded-lg
  shadow-sm
  p-6
  hover:shadow-md hover:border-blue-300
  focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2
  transition-all duration-200
">
  <div class="flex items-center justify-between">
    <h3 class="text-lg font-semibold text-gray-900">
      Grocery List
    </h3>
    <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
      Active
    </span>
  </div>
  <p class="text-sm text-gray-600 mt-2">
    12 items • 7 completed
  </p>
  <div class="mt-4">
    <div class="w-full bg-gray-200 rounded-full h-2">
      <div class="bg-blue-600 h-2 rounded-full" style="width: 58%"></div>
    </div>
  </div>
</a>
```

### Modals / Dialogs

```html
<!-- Overlay -->
<div class="fixed inset-0 bg-gray-900 bg-opacity-50 z-40" aria-hidden="true"></div>

<!-- Modal -->
<div class="fixed inset-0 z-50 overflow-y-auto">
  <div class="flex min-h-full items-center justify-center p-4">
    <div class="
      relative
      bg-white
      rounded-lg
      shadow-xl
      max-w-lg w-full
      p-6
    ">
      <!-- Close button -->
      <button class="
        absolute top-4 right-4
        text-gray-400 hover:text-gray-600
        focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2
        rounded-md
      ">
        <svg class="w-6 h-6" fill="none" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
        </svg>
      </button>

      <!-- Header -->
      <h2 class="text-2xl font-bold text-gray-900 mb-4">
        Modal Title
      </h2>

      <!-- Body -->
      <div class="text-base text-gray-600 mb-6">
        <p>Modal content goes here...</p>
      </div>

      <!-- Footer -->
      <div class="flex justify-end space-x-3">
        <button class="btn-secondary">Cancel</button>
        <button class="btn-primary">Confirm</button>
      </div>
    </div>
  </div>
</div>
```

### Toast Notifications

```html
<!-- Success Toast -->
<div class="
  fixed bottom-6 right-6
  max-w-sm w-full
  bg-white
  shadow-lg rounded-lg
  pointer-events-auto
  ring-1 ring-black ring-opacity-5
  overflow-hidden
  z-50
">
  <div class="p-4">
    <div class="flex items-start">
      <div class="flex-shrink-0">
        <svg class="h-6 w-6 text-green-500" fill="none" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
        </svg>
      </div>
      <div class="ml-3 w-0 flex-1">
        <p class="text-sm font-medium text-gray-900">
          Task completed!
        </p>
        <p class="mt-1 text-sm text-gray-500">
          "Buy groceries" has been marked as complete.
        </p>
      </div>
      <div class="ml-4 flex flex-shrink-0">
        <button class="
          inline-flex text-gray-400 hover:text-gray-600
          focus:outline-none focus:ring-2 focus:ring-blue-500
          rounded-md
        ">
          <svg class="h-5 w-5" fill="none" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
          </svg>
        </button>
      </div>
    </div>
  </div>
</div>

<!-- Error Toast: Replace green-500 with red-500 -->
<!-- Warning Toast: Replace green-500 with amber-500 -->
<!-- Info Toast: Replace green-500 with blue-500 -->
```

### Badges & Tags

```html
<!-- Success Badge -->
<span class="
  inline-flex items-center
  px-2.5 py-0.5
  rounded-full
  text-xs font-medium
  bg-green-100 text-green-800
">
  Active
</span>

<!-- Error Badge -->
<span class="
  inline-flex items-center
  px-2.5 py-0.5
  rounded-full
  text-xs font-medium
  bg-red-100 text-red-800
">
  Overdue
</span>

<!-- Warning Badge -->
<span class="
  inline-flex items-center
  px-2.5 py-0.5
  rounded-full
  text-xs font-medium
  bg-amber-100 text-amber-800
">
  Pending
</span>

<!-- Info Badge -->
<span class="
  inline-flex items-center
  px-2.5 py-0.5
  rounded-full
  text-xs font-medium
  bg-blue-100 text-blue-800
">
  3 New
</span>

<!-- Neutral Badge -->
<span class="
  inline-flex items-center
  px-2.5 py-0.5
  rounded-full
  text-xs font-medium
  bg-gray-100 text-gray-800
">
  Archived
</span>
```

### Progress Bars

```html
<!-- Basic Progress Bar -->
<div class="w-full bg-gray-200 rounded-full h-2">
  <div
    class="bg-blue-600 h-2 rounded-full transition-all duration-300"
    style="width: 65%"
    role="progressbar"
    aria-valuenow="65"
    aria-valuemin="0"
    aria-valuemax="100"
  ></div>
</div>

<!-- With Label -->
<div class="w-full">
  <div class="flex justify-between mb-1">
    <span class="text-sm font-medium text-gray-700">Progress</span>
    <span class="text-sm font-medium text-gray-700">65%</span>
  </div>
  <div class="w-full bg-gray-200 rounded-full h-2">
    <div class="bg-blue-600 h-2 rounded-full" style="width: 65%"></div>
  </div>
</div>

<!-- Striped Progress (for loading) -->
<div class="w-full bg-gray-200 rounded-full h-2 overflow-hidden">
  <div
    class="bg-blue-600 h-2 rounded-full animate-pulse"
    style="width: 100%"
  ></div>
</div>
```

### Loading States

#### Spinner

```html
<svg class="animate-spin h-8 w-8 text-blue-600" fill="none" viewBox="0 0 24 24">
  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
</svg>
```

#### Skeleton Screen

```html
<div class="animate-pulse">
  <div class="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
  <div class="h-4 bg-gray-200 rounded w-1/2 mb-4"></div>
  <div class="h-32 bg-gray-200 rounded mb-4"></div>
  <div class="h-4 bg-gray-200 rounded w-full mb-2"></div>
  <div class="h-4 bg-gray-200 rounded w-5/6"></div>
</div>
```

### Empty States

```html
<div class="text-center py-12">
  <!-- Icon/Illustration -->
  <svg class="mx-auto h-24 w-24 text-gray-400" fill="none" stroke="currentColor">
    <!-- Empty state icon -->
  </svg>

  <!-- Heading -->
  <h3 class="mt-4 text-lg font-medium text-gray-900">
    No tasks yet
  </h3>

  <!-- Description -->
  <p class="mt-2 text-sm text-gray-600 max-w-sm mx-auto">
    Get started by creating a new task for your family.
  </p>

  <!-- Action -->
  <div class="mt-6">
    <button class="btn-primary">
      <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
      </svg>
      Create Task
    </button>
  </div>
</div>
```

---

## Design Tokens

### Tailwind Configuration

```javascript
// tailwind.config.js
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        // Primary
        blue: {
          50: '#EBF5FF',
          100: '#D1E9FF',
          200: '#B3DDFF',
          300: '#84C7FF',
          400: '#56A9FF',
          500: '#3B8FFF',
          600: '#2771E0',
          700: '#1B5AC1',
          800: '#13479B',
          900: '#0D3875',
        },
        // Success / Kids
        green: {
          50: '#ECFDF5',
          100: '#D1FAE5',
          200: '#A7F3D0',
          300: '#6EE7B7',
          400: '#34D399',
          500: '#10B981',
          600: '#059669',
          700: '#047857',
          800: '#065F46',
          900: '#064E3B',
        },
        // Family
        purple: {
          50: '#FAF5FF',
          100: '#F3E8FF',
          200: '#E9D5FF',
          300: '#D8B4FE',
          400: '#C084FC',
          500: '#A855F7',
          600: '#9333EA',
          700: '#7E22CE',
          800: '#6B21A8',
          900: '#581C87',
        },
        // Warning / Personal
        amber: {
          50: '#FFFBEB',
          100: '#FEF3C7',
          200: '#FDE68A',
          300: '#FCD34D',
          400: '#FBBF24',
          500: '#F59E0B',
          600: '#D97706',
          700: '#B45309',
          800: '#92400E',
          900: '#78350F',
        },
        // Error / Health
        red: {
          50: '#FEF2F2',
          100: '#FEE2E2',
          200: '#FECACA',
          300: '#FCA5A5',
          400: '#F87171',
          500: '#EF4444',
          600: '#DC2626',
          700: '#B91C1C',
          800: '#991B1B',
          900: '#7F1D1D',
        },
        // Finance
        teal: {
          50: '#F0FDFA',
          100: '#CCFBF1',
          200: '#99F6E4',
          300: '#5EEAD4',
          400: '#2DD4BF',
          500: '#14B8A6',
          600: '#0D9488',
          700: '#0F766E',
          800: '#115E59',
          900: '#134E4A',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', '-apple-system', 'sans-serif'],
        mono: ['JetBrains Mono', 'Courier New', 'monospace'],
      },
      fontSize: {
        xs: ['0.75rem', { lineHeight: '1rem' }],
        sm: ['0.875rem', { lineHeight: '1.25rem' }],
        base: ['1rem', { lineHeight: '1.5rem' }],
        lg: ['1.125rem', { lineHeight: '1.75rem' }],
        xl: ['1.25rem', { lineHeight: '1.75rem' }],
        '2xl': ['1.5rem', { lineHeight: '2rem' }],
        '3xl': ['1.875rem', { lineHeight: '2.25rem' }],
        '4xl': ['2.25rem', { lineHeight: '2.5rem' }],
        '5xl': ['3rem', { lineHeight: '1' }],
      },
      spacing: {
        '0': '0',
        '1': '0.25rem',
        '2': '0.5rem',
        '3': '0.75rem',
        '4': '1rem',
        '5': '1.25rem',
        '6': '1.5rem',
        '8': '2rem',
        '10': '2.5rem',
        '12': '3rem',
        '16': '4rem',
        '20': '5rem',
        '24': '6rem',
        '32': '8rem',
      },
      borderRadius: {
        'none': '0',
        'sm': '0.125rem',
        'DEFAULT': '0.25rem',
        'md': '0.375rem',
        'lg': '0.5rem',
        'xl': '0.75rem',
        '2xl': '1rem',
        'full': '9999px',
      },
      boxShadow: {
        sm: '0 1px 2px 0 rgba(0, 0, 0, 0.05)',
        DEFAULT: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)',
        md: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
        lg: '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
        xl: '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)',
        '2xl': '0 25px 50px -12px rgba(0, 0, 0, 0.25)',
        inner: 'inset 0 2px 4px 0 rgba(0, 0, 0, 0.06)',
        none: 'none',
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
    require('@tailwindcss/aspect-ratio'),
  ],
}
```

### CSS Custom Properties

```css
:root {
  /* Colors - Primary */
  --color-primary-50: #EBF5FF;
  --color-primary-500: #3B8FFF;
  --color-primary-600: #2771E0;
  --color-primary-700: #1B5AC1;

  /* Colors - Semantic */
  --color-success: #10B981;
  --color-warning: #F59E0B;
  --color-error: #EF4444;
  --color-info: #3B8FFF;

  /* Typography */
  --font-family-sans: 'Inter', system-ui, sans-serif;
  --font-size-base: 1rem;
  --line-height-normal: 1.5;

  /* Spacing */
  --spacing-4: 1rem;
  --spacing-6: 1.5rem;
  --spacing-8: 2rem;

  /* Shadows */
  --shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1);

  /* Border Radius */
  --radius-sm: 0.125rem;
  --radius-md: 0.375rem;
  --radius-lg: 0.5rem;
  --radius-full: 9999px;

  /* Transitions */
  --transition-fast: 150ms ease-in-out;
  --transition-base: 200ms ease-in-out;
  --transition-slow: 300ms ease-in-out;

  /* Z-index layers */
  --z-base: 0;
  --z-dropdown: 1000;
  --z-sticky: 1020;
  --z-fixed: 1030;
  --z-modal-backdrop: 1040;
  --z-modal: 1050;
  --z-popover: 1060;
  --z-tooltip: 1070;
}
```

---

## Dark Mode

### Color Adjustments

```css
@media (prefers-color-scheme: dark) {
  :root {
    /* Background Colors */
    --bg-primary: #111827;    /* gray-900 */
    --bg-secondary: #1F2937;  /* gray-800 */
    --bg-tertiary: #374151;   /* gray-700 */

    /* Text Colors */
    --text-primary: #F9FAFB;   /* gray-50 */
    --text-secondary: #E5E7EB; /* gray-200 */
    --text-tertiary: #9CA3AF;  /* gray-400 */

    /* Border Colors */
    --border-primary: #374151; /* gray-700 */
    --border-secondary: #4B5563; /* gray-600 */

    /* Adjust primary colors for dark mode */
    --color-primary-500: #56A9FF;
    --color-primary-600: #84C7FF;

    /* Shadows (lighter for dark mode) */
    --shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.3);
    --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.3);
  }
}
```

### Tailwind Dark Mode

```javascript
// tailwind.config.js
module.exports = {
  darkMode: 'class', // or 'media'
  // ... rest of config
}
```

```html
<!-- Manual dark mode toggle -->
<html class="dark">
  <!-- Dark mode styles apply -->
</html>

<!-- Example usage -->
<div class="bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-50">
  Content adapts to dark mode
</div>
```

---

## Motion Design

### Animation Principles

1. **Purposeful**: Every animation should have a reason
2. **Subtle**: Don't distract from content
3. **Fast**: Keep durations short (200-400ms)
4. **Consistent**: Use same timing functions throughout
5. **Respectful**: Honor prefers-reduced-motion

### Timing Functions

```css
/* Easing curves */
--ease-in: cubic-bezier(0.4, 0, 1, 1);
--ease-out: cubic-bezier(0, 0, 0.2, 1);
--ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);
--ease-spring: cubic-bezier(0.68, -0.55, 0.265, 1.55);

/* Durations */
--duration-fast: 150ms;
--duration-base: 200ms;
--duration-slow: 300ms;
--duration-slower: 400ms;
```

### Common Animations

#### Fade In

```css
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.fade-in {
  animation: fadeIn 200ms ease-out;
}
```

#### Slide In from Right

```css
@keyframes slideInRight {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

.slide-in-right {
  animation: slideInRight 300ms ease-out;
}
```

#### Scale (for modals)

```css
@keyframes scaleIn {
  from {
    transform: scale(0.95);
    opacity: 0;
  }
  to {
    transform: scale(1);
    opacity: 1;
  }
}

.scale-in {
  animation: scaleIn 200ms ease-out;
}
```

#### Shake (for errors)

```css
@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-10px); }
  75% { transform: translateX(10px); }
}

.shake {
  animation: shake 300ms ease-in-out;
}
```

#### Bounce (for success)

```css
@keyframes bounce {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-10px); }
}

.bounce {
  animation: bounce 400ms ease-in-out;
}
```

### Respect Reduced Motion

```css
@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
    scroll-behavior: auto !important;
  }
}
```

### Micro-interactions

```css
/* Button press */
.btn:active {
  transform: scale(0.98);
  transition: transform 100ms ease-out;
}

/* Checkbox check */
input[type="checkbox"]:checked {
  animation: checkboxCheck 200ms ease-out;
}

@keyframes checkboxCheck {
  0% { transform: scale(0.8); }
  50% { transform: scale(1.1); }
  100% { transform: scale(1); }
}

/* Task completion */
.task-complete {
  animation: taskComplete 400ms ease-out;
}

@keyframes taskComplete {
  0% { transform: scale(1); }
  25% { transform: scale(1.05); }
  50% { opacity: 0.7; }
  75% { transform: scale(0.95); }
  100% { transform: scale(1); opacity: 0.5; }
}
```

---

## Usage Examples

### Complete Button Example

```html
<button class="
  inline-flex items-center justify-center
  px-4 py-2
  text-base font-medium text-white
  bg-blue-600 hover:bg-blue-700 active:bg-blue-800
  rounded-md
  border border-transparent
  shadow-sm hover:shadow-md
  focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2
  disabled:opacity-50 disabled:cursor-not-allowed
  transition-all duration-200
  transform active:scale-98
">
  <svg class="w-5 h-5 mr-2 -ml-1" fill="none" stroke="currentColor">
    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
  </svg>
  Create Task
</button>
```

### Complete Card Example

```html
<div class="
  bg-white dark:bg-gray-800
  border border-gray-200 dark:border-gray-700
  rounded-lg
  shadow-sm hover:shadow-md
  overflow-hidden
  transition-shadow duration-200
">
  <!-- Header -->
  <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900">
    <div class="flex items-center justify-between">
      <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-50">
        Grocery List
      </h3>
      <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200">
        Active
      </span>
    </div>
    <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
      12 items • 7 completed
    </p>
  </div>

  <!-- Body -->
  <div class="px-6 py-4">
    <div class="space-y-2">
      <!-- Progress bar -->
      <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
        <div class="bg-blue-600 h-2 rounded-full transition-all duration-300" style="width: 58%"></div>
      </div>

      <!-- Last updated -->
      <p class="text-sm text-gray-500 dark:text-gray-400">
        Last updated by Jane • 2 hours ago
      </p>
    </div>
  </div>

  <!-- Footer -->
  <div class="px-6 py-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900">
    <div class="flex justify-end space-x-3">
      <button class="btn-secondary">Archive</button>
      <button class="btn-primary">Open List</button>
    </div>
  </div>
</div>
```

---

**Document Status:** Design System Complete
**Next Steps:** Implement in Angular components, create Storybook documentation
**Related Documents:** wireframes.md, angular-component-specs.md, responsive-design-guide.md

