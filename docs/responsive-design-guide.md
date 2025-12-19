# Family Hub - Responsive Design Guide

**Version:** 1.0
**Date:** 2025-12-19
**Status:** Design Specification
**Author:** UI Designer (Claude Code)

---

## Table of Contents

1. [Breakpoint Strategy](#breakpoint-strategy)
2. [Mobile-First Approach](#mobile-first-approach)
3. [Responsive Patterns](#responsive-patterns)
4. [PWA Implementation](#pwa-implementation)
5. [Touch & Gesture Patterns](#touch--gesture-patterns)
6. [Cross-Device Continuity](#cross-device-continuity)

---

## Breakpoint Strategy

### Tailwind Breakpoints

```css
/* Mobile First Breakpoints */
--breakpoint-sm: 640px; /* Small devices (landscape phones) */
--breakpoint-md: 768px; /* Medium devices (tablets) */
--breakpoint-lg: 1024px; /* Large devices (desktops) */
--breakpoint-xl: 1280px; /* Extra large devices */
--breakpoint-2xl: 1536px; /* 2X large devices */
```

### Device Categories

**Mobile (< 640px)**

- Primary use case: Phones in portrait mode
- Bottom navigation
- Single column layouts
- Touch-optimized interactions
- Reduced data/imagery

**Tablet (640px - 1023px)**

- Primary use case: Tablets, large phones in landscape
- Side navigation (collapsible)
- Two-column layouts where appropriate
- Hybrid touch/mouse interactions

**Desktop (1024px+)**

- Primary use case: Laptops, desktops
- Persistent side navigation
- Multi-column layouts
- Mouse/keyboard-optimized
- Rich data visualizations

---

## Mobile-First Approach

### Base Styles (Mobile)

```css
/* Start with mobile styles */
.container {
  padding: 1rem;
  font-size: 0.875rem;
}

.grid {
  grid-template-columns: 1fr;
  gap: 1rem;
}

/* Then add larger screen styles */
@media (min-width: 768px) {
  .container {
    padding: 1.5rem;
    font-size: 1rem;
  }

  .grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 1.5rem;
  }
}

@media (min-width: 1024px) {
  .container {
    padding: 2rem;
  }

  .grid {
    grid-template-columns: repeat(3, 1fr);
    gap: 2rem;
  }
}
```

### Tailwind Responsive Classes

```html
<!-- Mobile-first approach with Tailwind -->
<div
  class="
  grid
  grid-cols-1
  md:grid-cols-2
  lg:grid-cols-3
  gap-4
  md:gap-6
  lg:gap-8
"
>
  <!-- Grid items -->
</div>

<!-- Text sizing -->
<h1 class="text-2xl md:text-3xl lg:text-4xl">Responsive Heading</h1>

<!-- Show/hide elements -->
<div class="hidden lg:block">Desktop only content</div>

<div class="lg:hidden">Mobile/tablet only content</div>
```

---

## Responsive Patterns

### Navigation Patterns

#### Mobile: Bottom Navigation

```html
<nav
  class="lg:hidden fixed bottom-0 inset-x-0 bg-white border-t border-gray-200 z-50"
>
  <div class="grid grid-cols-5 h-16">
    <a
      href="/dashboard"
      class="flex flex-col items-center justify-center text-gray-600 hover:text-blue-600"
    >
      <fh-icon name="home" size="md" />
      <span class="text-xs mt-1">Home</span>
    </a>
    <a
      href="/calendar"
      class="flex flex-col items-center justify-center text-gray-600 hover:text-blue-600"
    >
      <fh-icon name="calendar" size="md" />
      <span class="text-xs mt-1">Calendar</span>
    </a>
    <a
      href="/lists"
      class="flex flex-col items-center justify-center text-gray-600 hover:text-blue-600"
    >
      <fh-icon name="list-bullet" size="md" />
      <span class="text-xs mt-1">Lists</span>
    </a>
    <a
      href="/tasks"
      class="flex flex-col items-center justify-center text-gray-600 hover:text-blue-600"
    >
      <fh-icon name="check-circle" size="md" />
      <span class="text-xs mt-1">Tasks</span>
    </a>
    <a
      href="/profile"
      class="flex flex-col items-center justify-center text-gray-600 hover:text-blue-600"
    >
      <fh-icon name="user" size="md" />
      <span class="text-xs mt-1">Me</span>
    </a>
  </div>
</nav>
```

#### Desktop: Side Navigation

```html
<aside
  class="hidden lg:flex lg:flex-col lg:w-64 lg:fixed lg:inset-y-0 bg-gray-50 border-r border-gray-200"
>
  <div class="flex flex-col flex-1 p-4">
    <nav class="flex-1 space-y-1">
      <a
        href="/dashboard"
        class="flex items-center px-4 py-2 text-gray-700 hover:bg-gray-100 rounded-md"
      >
        <fh-icon name="home" size="md" class="mr-3" />
        Dashboard
      </a>
      <!-- More nav items -->
    </nav>
  </div>
</aside>

<!-- Main content with offset for sidebar -->
<main class="lg:pl-64">
  <!-- Page content -->
</main>
```

### Layout Patterns

#### Dashboard Grid

```html
<!-- Responsive dashboard grid -->
<div
  class="
  grid
  grid-cols-1
  md:grid-cols-2
  lg:grid-cols-3
  gap-4
  md:gap-6
"
>
  <!-- Widget 1: Full width on mobile, half on tablet, third on desktop -->
  <div class="bg-white p-6 rounded-lg shadow">
    <h3>Today's Schedule</h3>
    <!-- Content -->
  </div>

  <!-- Widget 2 -->
  <div class="bg-white p-6 rounded-lg shadow">
    <h3>Upcoming Events</h3>
    <!-- Content -->
  </div>

  <!-- Widget 3 -->
  <div class="bg-white p-6 rounded-lg shadow md:col-span-2 lg:col-span-1">
    <h3>Tasks & Chores</h3>
    <!-- Content -->
  </div>
</div>
```

#### List to Table Pattern

```html
<!-- Mobile: Stacked cards -->
<div class="lg:hidden space-y-4">
  @for (item of items) {
  <div class="bg-white p-4 rounded-lg shadow">
    <h3 class="font-semibold">{{ item.title }}</h3>
    <p class="text-sm text-gray-600">{{ item.description }}</p>
    <div class="mt-2 flex justify-between items-center">
      <span class="text-xs text-gray-500">{{ item.date }}</span>
      <fh-badge [variant]="item.status">{{ item.status }}</fh-badge>
    </div>
  </div>
  }
</div>

<!-- Desktop: Table -->
<div class="hidden lg:block">
  <table class="min-w-full divide-y divide-gray-200">
    <thead class="bg-gray-50">
      <tr>
        <th>Title</th>
        <th>Description</th>
        <th>Date</th>
        <th>Status</th>
      </tr>
    </thead>
    <tbody>
      @for (item of items) {
      <tr>
        <td>{{ item.title }}</td>
        <td>{{ item.description }}</td>
        <td>{{ item.date }}</td>
        <td><fh-badge [variant]="item.status">{{ item.status }}</fh-badge></td>
      </tr>
      }
    </tbody>
  </table>
</div>
```

### Modal Patterns

```html
<!-- Full screen on mobile, centered on desktop -->
<div
  class="
  fixed inset-0
  lg:flex lg:items-center lg:justify-center
  z-50
"
>
  <div
    class="
    bg-white
    h-full w-full
    lg:h-auto lg:max-w-2xl lg:rounded-lg lg:shadow-xl
    overflow-y-auto
    p-6
  "
  >
    <!-- Modal content -->
  </div>
</div>
```

---

## PWA Implementation

### Service Worker Configuration

```typescript
// ngsw-config.json
{
  "index": "/index.html",
  "assetGroups": [
    {
      "name": "app",
      "installMode": "prefetch",
      "resources": {
        "files": [
          "/favicon.ico",
          "/index.html",
          "/manifest.webmanifest",
          "/*.css",
          "/*.js"
        ]
      }
    },
    {
      "name": "assets",
      "installMode": "lazy",
      "updateMode": "prefetch",
      "resources": {
        "files": [
          "/assets/**",
          "/*.(eot|svg|cur|jpg|png|webp|gif|otf|ttf|woff|woff2)"
        ]
      }
    }
  ],
  "dataGroups": [
    {
      "name": "api-cache",
      "urls": ["/api/**"],
      "cacheConfig": {
        "strategy": "freshness",
        "maxSize": 100,
        "maxAge": "1h",
        "timeout": "5s"
      }
    }
  ]
}
```

### Web App Manifest

```json
{
  "name": "Family Hub",
  "short_name": "FamilyHub",
  "description": "Privacy-first family organization platform",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#ffffff",
  "theme_color": "#3B8FFF",
  "orientation": "portrait-primary",
  "icons": [
    {
      "src": "/assets/icons/icon-72x72.png",
      "sizes": "72x72",
      "type": "image/png",
      "purpose": "any maskable"
    },
    {
      "src": "/assets/icons/icon-96x96.png",
      "sizes": "96x96",
      "type": "image/png"
    },
    {
      "src": "/assets/icons/icon-128x128.png",
      "sizes": "128x128",
      "type": "image/png"
    },
    {
      "src": "/assets/icons/icon-144x144.png",
      "sizes": "144x144",
      "type": "image/png"
    },
    {
      "src": "/assets/icons/icon-152x152.png",
      "sizes": "152x152",
      "type": "image/png"
    },
    {
      "src": "/assets/icons/icon-192x192.png",
      "sizes": "192x192",
      "type": "image/png"
    },
    {
      "src": "/assets/icons/icon-384x384.png",
      "sizes": "384x384",
      "type": "image/png"
    },
    {
      "src": "/assets/icons/icon-512x512.png",
      "sizes": "512x512",
      "type": "image/png"
    }
  ]
}
```

### Install Prompt

```typescript
import { Component, OnInit } from "@angular/core";

@Component({
  selector: "fh-install-prompt",
  template: `
    @if (showInstallPrompt) {
    <div
      class="fixed bottom-20 inset-x-4 lg:bottom-6 lg:right-6 lg:left-auto lg:max-w-sm bg-white shadow-lg rounded-lg p-4 border border-gray-200"
    >
      <div class="flex items-start">
        <div class="flex-shrink-0">
          <fh-icon name="download" size="md" class="text-blue-600" />
        </div>
        <div class="ml-3 flex-1">
          <h3 class="text-sm font-medium text-gray-900">Install Family Hub</h3>
          <p class="mt-1 text-sm text-gray-500">
            Install our app for quick access and offline functionality.
          </p>
          <div class="mt-3 flex space-x-2">
            <button (click)="installPwa()" class="btn-primary btn-sm">
              Install
            </button>
            <button (click)="dismissPrompt()" class="btn-secondary btn-sm">
              Not now
            </button>
          </div>
        </div>
      </div>
    </div>
    }
  `,
})
export class InstallPromptComponent implements OnInit {
  showInstallPrompt = false;
  private deferredPrompt: any;

  ngOnInit(): void {
    window.addEventListener("beforeinstallprompt", (e) => {
      e.preventDefault();
      this.deferredPrompt = e;

      // Check if user previously dismissed
      const dismissed = localStorage.getItem("install-prompt-dismissed");
      if (!dismissed) {
        this.showInstallPrompt = true;
      }
    });
  }

  async installPwa(): Promise<void> {
    if (this.deferredPrompt) {
      this.deferredPrompt.prompt();
      const { outcome } = await this.deferredPrompt.userChoice;

      if (outcome === "accepted") {
        console.log("User accepted install");
      }

      this.deferredPrompt = null;
      this.showInstallPrompt = false;
    }
  }

  dismissPrompt(): void {
    this.showInstallPrompt = false;
    localStorage.setItem("install-prompt-dismissed", "true");
  }
}
```

---

## Touch & Gesture Patterns

### Touch Target Sizing

```css
/* Minimum touch target sizes */
.touch-target {
  min-width: 44px;
  min-height: 44px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

/* Android recommendation */
@media (max-width: 639px) {
  .touch-target {
    min-width: 48px;
    min-height: 48px;
  }
}
```

### Swipe Gestures

```typescript
import {
  Directive,
  ElementRef,
  EventEmitter,
  HostListener,
  Output,
} from "@angular/core";

@Directive({
  selector: "[fhSwipe]",
  standalone: true,
})
export class SwipeDirective {
  @Output() swipeLeft = new EventEmitter<void>();
  @Output() swipeRight = new EventEmitter<void>();

  private swipeThreshold = 50;
  private touchStartX = 0;
  private touchStartY = 0;

  constructor(private el: ElementRef) {}

  @HostListener("touchstart", ["$event"])
  onTouchStart(event: TouchEvent): void {
    this.touchStartX = event.changedTouches[0].screenX;
    this.touchStartY = event.changedTouches[0].screenY;
  }

  @HostListener("touchend", ["$event"])
  onTouchEnd(event: TouchEvent): void {
    const touchEndX = event.changedTouches[0].screenX;
    const touchEndY = event.changedTouches[0].screenY;

    const deltaX = touchEndX - this.touchStartX;
    const deltaY = touchEndY - this.touchStartY;

    // Only trigger if horizontal swipe is dominant
    if (Math.abs(deltaX) > Math.abs(deltaY)) {
      if (deltaX > this.swipeThreshold) {
        this.swipeRight.emit();
      } else if (deltaX < -this.swipeThreshold) {
        this.swipeLeft.emit();
      }
    }
  }
}
```

**Usage**:

```html
<div
  fhSwipe
  (swipeLeft)="deleteItem()"
  (swipeRight)="completeItem()"
  class="swipeable-item"
>
  <div class="item-content">
    <!-- Item content -->
  </div>
</div>
```

### Pull to Refresh

```typescript
import {
  Directive,
  ElementRef,
  EventEmitter,
  HostListener,
  Output,
} from "@angular/core";

@Directive({
  selector: "[fhPullToRefresh]",
  standalone: true,
})
export class PullToRefreshDirective {
  @Output() refresh = new EventEmitter<void>();

  private pullThreshold = 80;
  private touchStartY = 0;
  private isPulling = false;
  private scrollTop = 0;

  constructor(private el: ElementRef) {}

  @HostListener("touchstart", ["$event"])
  onTouchStart(event: TouchEvent): void {
    this.scrollTop = this.el.nativeElement.scrollTop;
    this.touchStartY = event.touches[0].clientY;
  }

  @HostListener("touchmove", ["$event"])
  onTouchMove(event: TouchEvent): void {
    if (this.scrollTop === 0) {
      const currentY = event.touches[0].clientY;
      const pullDistance = currentY - this.touchStartY;

      if (pullDistance > 0) {
        this.isPulling = true;
        // Show refresh indicator
      }
    }
  }

  @HostListener("touchend", ["$event"])
  onTouchEnd(event: TouchEvent): void {
    if (this.isPulling) {
      const currentY = event.changedTouches[0].clientY;
      const pullDistance = currentY - this.touchStartY;

      if (pullDistance > this.pullThreshold) {
        this.refresh.emit();
      }

      this.isPulling = false;
    }
  }
}
```

### Long Press

```typescript
import {
  Directive,
  ElementRef,
  EventEmitter,
  HostListener,
  Output,
} from "@angular/core";

@Directive({
  selector: "[fhLongPress]",
  standalone: true,
})
export class LongPressDirective {
  @Output() longPress = new EventEmitter<void>();

  private longPressTimeout: any;
  private longPressDuration = 500; // ms

  @HostListener("touchstart", ["$event"])
  @HostListener("mousedown", ["$event"])
  onPointerDown(event: Event): void {
    this.longPressTimeout = setTimeout(() => {
      this.longPress.emit();
    }, this.longPressDuration);
  }

  @HostListener("touchend")
  @HostListener("mouseup")
  @HostListener("mouseleave")
  onPointerUp(): void {
    clearTimeout(this.longPressTimeout);
  }
}
```

---

## Cross-Device Continuity

### State Synchronization

```typescript
import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";

@Injectable({ providedIn: "root" })
export class StateSyncService {
  private syncState = new BehaviorSubject<any>(null);

  syncState$ = this.syncState.asObservable();

  saveState(key: string, data: any): void {
    const state = {
      key,
      data,
      timestamp: Date.now(),
      device: this.getDeviceInfo(),
    };

    // Save to localStorage for persistence
    localStorage.setItem(`state-${key}`, JSON.stringify(state));

    // Emit to subscribers
    this.syncState.next(state);

    // Sync to server
    this.syncToServer(state);
  }

  loadState(key: string): any {
    const stored = localStorage.getItem(`state-${key}`);
    return stored ? JSON.parse(stored) : null;
  }

  private getDeviceInfo(): string {
    return `${navigator.userAgent}`;
  }

  private syncToServer(state: any): void {
    // Implement server sync logic
    // This would use your GraphQL API to sync state
  }
}
```

### Responsive Images

```html
<!-- Responsive image with different sizes -->
<img
  srcset="
    /assets/images/hero-400w.jpg   400w,
    /assets/images/hero-800w.jpg   800w,
    /assets/images/hero-1200w.jpg 1200w
  "
  sizes="
    (max-width: 640px) 100vw,
    (max-width: 1024px) 50vw,
    33vw
  "
  src="/assets/images/hero-800w.jpg"
  alt="Hero image"
  loading="lazy"
/>

<!-- Picture element for different formats -->
<picture>
  <source type="image/webp" srcset="/assets/images/hero.webp" />
  <source type="image/jpeg" srcset="/assets/images/hero.jpg" />
  <img src="/assets/images/hero.jpg" alt="Hero image" loading="lazy" />
</picture>
```

---

**Document Status:** Responsive Design Guide Complete
**Next Steps:** Test on real devices, optimize for performance
**Related Documents:** design-system.md, interaction-design-guide.md
