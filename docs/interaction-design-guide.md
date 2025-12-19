# Family Hub - Interaction Design Guide

**Version:** 1.0
**Date:** 2025-12-19
**Status:** Design Specification
**Author:** UI Designer (Claude Code)

---

## Table of Contents

1. [Micro-interactions](#micro-interactions)
2. [Animation Specifications](#animation-specifications)
3. [Gesture Patterns](#gesture-patterns)
4. [Real-time Update Patterns](#real-time-update-patterns)
5. [Gamification UI](#gamification-ui)
6. [Loading & Empty States](#loading--empty-states)

---

## Micro-interactions

### Button Press Feedback

```typescript
// Angular component with active state
@Component({
  selector: 'fh-button',
  template: `
    <button
      [class]="buttonClasses"
      (mousedown)="onPress()"
      (mouseup)="onRelease()"
      (mouseleave)="onRelease()"
    >
      <ng-content />
    </button>
  `,
  styles: [`
    button {
      transform: scale(1);
      transition: transform 100ms ease-out;
    }

    button.pressed {
      transform: scale(0.98);
    }

    button:active {
      transform: scale(0.98);
    }
  `]
})
export class ButtonComponent {
  isPressed = false;

  get buttonClasses(): string {
    return this.isPressed ? 'pressed' : '';
  }

  onPress(): void {
    this.isPressed = true;
  }

  onRelease(): void {
    this.isPressed = false;
  }
}
```

### Checkbox Animation

```css
/* Checkbox check animation */
input[type="checkbox"] {
  appearance: none;
  width: 1rem;
  height: 1rem;
  border: 2px solid #d1d5db;
  border-radius: 0.25rem;
  background-color: white;
  cursor: pointer;
  position: relative;
  transition: all 200ms ease;
}

input[type="checkbox"]:checked {
  background-color: #3b8fff;
  border-color: #3b8fff;
  animation: checkboxCheck 200ms ease-out;
}

input[type="checkbox"]:checked::after {
  content: '';
  position: absolute;
  left: 4px;
  top: 1px;
  width: 4px;
  height: 8px;
  border: solid white;
  border-width: 0 2px 2px 0;
  transform: rotate(45deg);
  animation: checkmarkDraw 200ms ease-out;
}

@keyframes checkboxCheck {
  0% {
    transform: scale(0.8);
  }
  50% {
    transform: scale(1.1);
  }
  100% {
    transform: scale(1);
  }
}

@keyframes checkmarkDraw {
  0% {
    height: 0;
  }
  100% {
    height: 8px;
  }
}
```

### Toggle Switch Animation

```css
.toggle-switch {
  position: relative;
  display: inline-flex;
  height: 1.5rem;
  width: 2.75rem;
  flex-shrink: 0;
  cursor: pointer;
  rounded: full;
  border: 2px solid transparent;
  background-color: #d1d5db;
  transition: background-color 200ms ease-in-out;
}

.toggle-switch.checked {
  background-color: #3b8fff;
}

.toggle-switch-handle {
  pointer-events: none;
  display: inline-block;
  height: 1.25rem;
  width: 1.25rem;
  transform: translateX(0);
  border-radius: 9999px;
  background-color: white;
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1);
  transition: transform 200ms ease-in-out;
}

.toggle-switch.checked .toggle-switch-handle {
  transform: translateX(1.25rem);
}
```

### Input Focus Animation

```css
.input-field {
  border: 2px solid #d1d5db;
  border-radius: 0.375rem;
  padding: 0.5rem 0.75rem;
  transition: all 200ms ease;
  position: relative;
}

.input-field:focus {
  border-color: #3b8fff;
  outline: none;
  box-shadow: 0 0 0 3px rgba(59, 143, 255, 0.1);
}

/* Label animation */
.input-wrapper {
  position: relative;
}

.floating-label {
  position: absolute;
  left: 0.75rem;
  top: 0.5rem;
  color: #9ca3af;
  pointer-events: none;
  transition: all 200ms ease;
  transform-origin: left top;
}

.input-field:focus ~ .floating-label,
.input-field:not(:placeholder-shown) ~ .floating-label {
  transform: translateY(-1.25rem) scale(0.875);
  color: #3b8fff;
}
```

### Hover Card Lift

```css
.card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  transition: all 200ms ease;
}

.card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  border-color: #3b8fff;
}
```

---

## Animation Specifications

### Page Transitions

```typescript
import { trigger, transition, style, animate } from '@angular/animations';

export const fadeInOut = trigger('fadeInOut', [
  transition(':enter', [
    style({ opacity: 0 }),
    animate('300ms ease-out', style({ opacity: 1 })),
  ]),
  transition(':leave', [
    animate('200ms ease-in', style({ opacity: 0 })),
  ]),
]);

export const slideInRight = trigger('slideInRight', [
  transition(':enter', [
    style({ transform: 'translateX(100%)', opacity: 0 }),
    animate('300ms ease-out', style({ transform: 'translateX(0)', opacity: 1 })),
  ]),
  transition(':leave', [
    animate('200ms ease-in', style({ transform: 'translateX(100%)', opacity: 0 })),
  ]),
]);

export const scaleIn = trigger('scaleIn', [
  transition(':enter', [
    style({ transform: 'scale(0.95)', opacity: 0 }),
    animate('200ms ease-out', style({ transform: 'scale(1)', opacity: 1 })),
  ]),
  transition(':leave', [
    animate('150ms ease-in', style({ transform: 'scale(0.95)', opacity: 0 })),
  ]),
]);
```

### List Item Animations

```typescript
export const listAnimation = trigger('listAnimation', [
  transition('* => *', [
    query(':enter', [
      style({ opacity: 0, transform: 'translateY(-10px)' }),
      stagger(50, [
        animate('200ms ease-out', style({ opacity: 1, transform: 'translateY(0)' })),
      ]),
    ], { optional: true }),
  ]),
]);

// Usage in component
@Component({
  selector: 'fh-task-list',
  animations: [listAnimation],
  template: `
    <div [@listAnimation]="tasks.length">
      @for (task of tasks; track task.id) {
        <fh-task-item [task]="task" />
      }
    </div>
  `,
})
export class TaskListComponent {
  @Input() tasks: Task[] = [];
}
```

### Task Completion Animation

```typescript
export const taskComplete = trigger('taskComplete', [
  transition('incomplete => complete', [
    animate('400ms ease-out', keyframes([
      style({ transform: 'scale(1)', offset: 0 }),
      style({ transform: 'scale(1.05)', offset: 0.25 }),
      style({ opacity: 0.7, offset: 0.5 }),
      style({ transform: 'scale(0.95)', offset: 0.75 }),
      style({ transform: 'scale(1)', opacity: 0.5, offset: 1 }),
    ])),
  ]),
]);

// Component usage
@Component({
  selector: 'fh-task-item',
  animations: [taskComplete],
  template: `
    <div [@taskComplete]="task.status">
      <!-- Task content -->
    </div>
  `,
})
export class TaskItemComponent {
  @Input() task!: Task;
}
```

### Success Confetti Animation

```typescript
import confetti from 'canvas-confetti';

@Injectable({ providedIn: 'root' })
export class ConfettiService {
  celebrate(): void {
    confetti({
      particleCount: 100,
      spread: 70,
      origin: { y: 0.6 },
      colors: ['#3b8fff', '#10b981', '#a855f7', '#f59e0b'],
    });
  }

  achievementUnlocked(): void {
    const duration = 3000;
    const end = Date.now() + duration;

    const frame = () => {
      confetti({
        particleCount: 2,
        angle: 60,
        spread: 55,
        origin: { x: 0 },
        colors: ['#3b8fff', '#10b981'],
      });
      confetti({
        particleCount: 2,
        angle: 120,
        spread: 55,
        origin: { x: 1 },
        colors: ['#a855f7', '#f59e0b'],
      });

      if (Date.now() < end) {
        requestAnimationFrame(frame);
      }
    };

    frame();
  }
}

// Component usage
@Component({
  selector: 'fh-task-complete-button',
  template: `
    <fh-button (clicked)="completeTask()">
      Mark Complete
    </fh-button>
  `,
})
export class TaskCompleteButtonComponent {
  constructor(private confetti: ConfettiService) {}

  completeTask(): void {
    // Complete task logic
    this.confetti.celebrate();
  }
}
```

---

## Gesture Patterns

### Swipe Actions

```typescript
@Component({
  selector: 'fh-swipeable-item',
  template: `
    <div
      class="swipeable-container"
      fhSwipe
      (swipeLeft)="onSwipeLeft()"
      (swipeRight)="onSwipeRight()"
    >
      <!-- Background actions (revealed on swipe) -->
      <div class="swipe-actions-left">
        <button class="action-button complete">
          <fh-icon name="check" />
          Complete
        </button>
      </div>

      <div class="swipe-actions-right">
        <button class="action-button delete">
          <fh-icon name="trash" />
          Delete
        </button>
      </div>

      <!-- Main content -->
      <div class="item-content" [style.transform]="'translateX(' + translateX + 'px)'">
        <ng-content />
      </div>
    </div>
  `,
  styles: [`
    .swipeable-container {
      position: relative;
      overflow: hidden;
    }

    .swipe-actions-left,
    .swipe-actions-right {
      position: absolute;
      top: 0;
      bottom: 0;
      width: 80px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .swipe-actions-left {
      left: 0;
      background: #10b981;
    }

    .swipe-actions-right {
      right: 0;
      background: #ef4444;
    }

    .item-content {
      position: relative;
      background: white;
      transition: transform 200ms ease;
    }
  `]
})
export class SwipeableItemComponent {
  translateX = 0;

  onSwipeLeft(): void {
    // Reveal delete action
    this.translateX = -80;
  }

  onSwipeRight(): void {
    // Reveal complete action
    this.translateX = 80;
  }
}
```

### Drag and Drop

```typescript
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';

@Component({
  selector: 'fh-sortable-list',
  template: `
    <div
      cdkDropList
      (cdkDropListDropped)="drop($event)"
      class="sortable-list"
    >
      @for (item of items; track item.id) {
        <div
          cdkDrag
          class="sortable-item"
        >
          <div class="drag-handle" cdkDragHandle>
            <fh-icon name="bars-3" size="sm" />
          </div>
          <div class="item-content">
            {{ item.title }}
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .sortable-item {
      display: flex;
      align-items: center;
      padding: 1rem;
      background: white;
      border: 1px solid #e5e7eb;
      border-radius: 0.5rem;
      margin-bottom: 0.5rem;
      cursor: move;
    }

    .sortable-item.cdk-drag-preview {
      box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
      transform: rotate(2deg);
    }

    .sortable-item.cdk-drag-animating {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }

    .cdk-drop-list-dragging .sortable-item:not(.cdk-drag-placeholder) {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }

    .drag-handle {
      margin-right: 0.75rem;
      color: #9ca3af;
      cursor: grab;
    }

    .drag-handle:active {
      cursor: grabbing;
    }
  `]
})
export class SortableListComponent {
  @Input() items: any[] = [];
  @Output() orderChanged = new EventEmitter<any[]>();

  drop(event: CdkDragDrop<any[]>): void {
    moveItemInArray(this.items, event.previousIndex, event.currentIndex);
    this.orderChanged.emit(this.items);
  }
}
```

---

## Real-time Update Patterns

### Optimistic UI Updates

```typescript
import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private tasks = signal<Task[]>([]);

  tasks$ = this.tasks.asReadonly();

  async toggleComplete(taskId: string): Promise<void> {
    // 1. Optimistically update UI
    const currentTasks = this.tasks();
    const taskIndex = currentTasks.findIndex(t => t.id === taskId);

    if (taskIndex !== -1) {
      const updatedTasks = [...currentTasks];
      updatedTasks[taskIndex] = {
        ...updatedTasks[taskIndex],
        completed: !updatedTasks[taskIndex].completed,
        completedAt: new Date(),
      };

      this.tasks.set(updatedTasks);

      try {
        // 2. Send request to server
        await this.api.toggleTaskComplete(taskId);

        // 3. Success - no action needed (already updated)
      } catch (error) {
        // 4. Rollback on error
        this.tasks.set(currentTasks);
        this.toastService.error('Failed to update task', 'Please try again');
      }
    }
  }
}
```

### Live Collaboration Indicators

```typescript
@Component({
  selector: 'fh-collaboration-indicator',
  template: `
    <div class="flex items-center space-x-2">
      @for (user of activeUsers(); track user.id) {
        <div
          class="relative"
          [title]="user.name + ' is viewing'"
        >
          <img
            [src]="user.avatar"
            [alt]="user.name"
            class="w-8 h-8 rounded-full border-2 border-white shadow"
          />
          <span class="absolute bottom-0 right-0 w-3 h-3 bg-green-500 border-2 border-white rounded-full"></span>
        </div>
      }

      @if (activeUsers().length > 3) {
        <div class="flex items-center justify-center w-8 h-8 bg-gray-200 rounded-full text-xs font-medium">
          +{{ activeUsers().length - 3 }}
        </div>
      }
    </div>
  `,
})
export class CollaborationIndicatorComponent {
  activeUsers = signal<User[]>([]);

  constructor(private realtimeService: RealtimeService) {
    this.realtimeService.activeUsers$.subscribe(users => {
      this.activeUsers.set(users);
    });
  }
}
```

### Real-time Notifications

```typescript
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class WebSocketService {
  private socket!: WebSocket;
  private messages$ = new Subject<any>();

  connect(url: string): void {
    this.socket = new WebSocket(url);

    this.socket.onmessage = (event) => {
      const message = JSON.parse(event.data);
      this.messages$.next(message);
    };

    this.socket.onerror = (error) => {
      console.error('WebSocket error:', error);
    };
  }

  onMessage(): Observable<any> {
    return this.messages$.asObservable();
  }

  send(message: any): void {
    if (this.socket.readyState === WebSocket.OPEN) {
      this.socket.send(JSON.stringify(message));
    }
  }
}

// Component usage
@Component({
  selector: 'fh-notification-listener',
  template: ``,
})
export class NotificationListenerComponent implements OnInit {
  constructor(
    private ws: WebSocketService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.ws.onMessage().subscribe(message => {
      if (message.type === 'TASK_ASSIGNED') {
        this.toastService.info(
          'New Task',
          `${message.assigner} assigned you: ${message.taskTitle}`
        );
      }
    });
  }
}
```

---

## Gamification UI

### Points Display

```typescript
@Component({
  selector: 'fh-points-display',
  template: `
    <div class="points-container">
      <div class="points-badge">
        <fh-icon name="star" class="text-amber-500" />
        <span class="points-value" [@countUp]="points()">
          {{ displayPoints }}
        </span>
      </div>

      @if (pointsEarned > 0) {
        <div class="points-earned" [@slideUp]>
          +{{ pointsEarned }}
        </div>
      }
    </div>
  `,
  styles: [`
    .points-container {
      position: relative;
      display: inline-flex;
      align-items: center;
    }

    .points-badge {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem 1rem;
      background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
      border-radius: 9999px;
      border: 2px solid #f59e0b;
      box-shadow: 0 4px 6px -1px rgba(245, 158, 11, 0.2);
    }

    .points-value {
      font-size: 1.125rem;
      font-weight: 700;
      color: #92400e;
    }

    .points-earned {
      position: absolute;
      top: -1rem;
      right: -0.5rem;
      background: #10b981;
      color: white;
      font-weight: 600;
      padding: 0.25rem 0.5rem;
      border-radius: 0.375rem;
      font-size: 0.875rem;
    }
  `],
  animations: [
    trigger('countUp', [
      transition('* => *', [
        // Animate number change
      ]),
    ]),
    trigger('slideUp', [
      transition(':enter', [
        style({ transform: 'translateY(20px)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateY(0)', opacity: 1 })),
      ]),
      transition(':leave', [
        animate('300ms ease-in', style({ transform: 'translateY(-20px)', opacity: 0 })),
      ]),
    ]),
  ],
})
export class PointsDisplayComponent {
  points = signal(0);
  pointsEarned = 0;
  displayPoints = 0;

  addPoints(amount: number): void {
    this.pointsEarned = amount;
    const newTotal = this.points() + amount;

    // Animate count up
    this.animateCountUp(this.points(), newTotal);

    this.points.set(newTotal);

    // Hide earned badge after 2 seconds
    setTimeout(() => {
      this.pointsEarned = 0;
    }, 2000);
  }

  private animateCountUp(start: number, end: number): void {
    const duration = 500;
    const increment = (end - start) / (duration / 16);
    let current = start;

    const timer = setInterval(() => {
      current += increment;
      if (current >= end) {
        this.displayPoints = end;
        clearInterval(timer);
      } else {
        this.displayPoints = Math.floor(current);
      }
    }, 16);
  }
}
```

### Achievement Unlock

```typescript
@Component({
  selector: 'fh-achievement-unlock',
  template: `
    @if (showAchievement) {
      <div class="achievement-modal" [@fadeIn]>
        <div class="achievement-content" [@scaleIn]>
          <div class="achievement-icon">
            <fh-icon name="trophy" size="xl" class="text-amber-500" />
          </div>

          <h2 class="achievement-title">Achievement Unlocked!</h2>

          <div class="achievement-badge">
            <img [src]="achievement.icon" [alt]="achievement.name" />
          </div>

          <h3 class="achievement-name">{{ achievement.name }}</h3>
          <p class="achievement-description">{{ achievement.description }}</p>

          <div class="achievement-reward">
            <fh-icon name="star" class="text-amber-500" />
            <span>+{{ achievement.points }} points</span>
          </div>

          <fh-button variant="primary" (clicked)="close()">
            Awesome!
          </fh-button>
        </div>
      </div>
    }
  `,
  styles: [`
    .achievement-modal {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.7);
      display: flex;
      align-items: center;
      justify-center;
      z-index: 9999;
    }

    .achievement-content {
      background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
      padding: 2rem;
      border-radius: 1rem;
      max-width: 400px;
      text-align: center;
      box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.3);
    }

    .achievement-icon {
      margin: 0 auto 1rem;
      width: 80px;
      height: 80px;
      display: flex;
      align-items: center;
      justify-center;
      background: white;
      border-radius: 50%;
      animation: pulse 1s ease-in-out infinite;
    }

    @keyframes pulse {
      0%, 100% { transform: scale(1); }
      50% { transform: scale(1.1); }
    }

    .achievement-badge {
      margin: 1.5rem auto;
      width: 120px;
      height: 120px;
    }

    .achievement-badge img {
      width: 100%;
      height: 100%;
      object-fit: contain;
      filter: drop-shadow(0 4px 6px rgba(0, 0, 0, 0.2));
    }

    .achievement-name {
      font-size: 1.5rem;
      font-weight: 700;
      color: #92400e;
      margin-bottom: 0.5rem;
    }

    .achievement-description {
      color: #78350f;
      margin-bottom: 1.5rem;
    }

    .achievement-reward {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      font-size: 1.125rem;
      font-weight: 600;
      color: #92400e;
      margin-bottom: 1.5rem;
    }
  `],
})
export class AchievementUnlockComponent {
  @Input() achievement!: Achievement;
  showAchievement = false;

  constructor(private confetti: ConfettiService) {}

  show(): void {
    this.showAchievement = true;
    this.confetti.achievementUnlocked();
  }

  close(): void {
    this.showAchievement = false;
  }
}
```

---

## Loading & Empty States

### Skeleton Screens

```typescript
@Component({
  selector: 'fh-task-skeleton',
  template: `
    <div class="animate-pulse space-y-4">
      @for (i of [1, 2, 3]; track i) {
        <div class="flex items-center space-x-4">
          <div class="w-4 h-4 bg-gray-200 rounded"></div>
          <div class="flex-1 space-y-2">
            <div class="h-4 bg-gray-200 rounded w-3/4"></div>
            <div class="h-3 bg-gray-200 rounded w-1/2"></div>
          </div>
        </div>
      }
    </div>
  `,
})
export class TaskSkeletonComponent {}
```

### Empty State Component

```typescript
@Component({
  selector: 'fh-empty-state',
  template: `
    <div class="empty-state">
      <!-- Icon/Illustration -->
      <div class="empty-state-icon">
        @if (illustration) {
          <img [src]="illustration" [alt]="title" />
        } @else {
          <fh-icon [name]="icon" size="2xl" class="text-gray-400" />
        }
      </div>

      <!-- Heading -->
      <h3 class="empty-state-title">
        {{ title }}
      </h3>

      <!-- Description -->
      <p class="empty-state-description">
        {{ description }}
      </p>

      <!-- Action -->
      @if (actionText) {
        <fh-button
          variant="primary"
          [iconLeft]="actionIcon"
          (clicked)="action.emit()"
        >
          {{ actionText }}
        </fh-button>
      }
    </div>
  `,
  styles: [`
    .empty-state {
      text-align: center;
      padding: 3rem 1rem;
      max-width: 28rem;
      margin: 0 auto;
    }

    .empty-state-icon {
      margin: 0 auto 1.5rem;
      width: 6rem;
      height: 6rem;
      display: flex;
      align-items: center;
      justify-center;
    }

    .empty-state-icon img {
      width: 100%;
      height: 100%;
      object-fit: contain;
    }

    .empty-state-title {
      font-size: 1.125rem;
      font-weight: 600;
      color: #111827;
      margin-bottom: 0.5rem;
    }

    .empty-state-description {
      color: #6b7280;
      margin-bottom: 1.5rem;
      line-height: 1.5;
    }
  `],
})
export class EmptyStateComponent {
  @Input() icon = 'folder-open';
  @Input() illustration?: string;
  @Input() title!: string;
  @Input() description!: string;
  @Input() actionText?: string;
  @Input() actionIcon = 'plus';

  @Output() action = new EventEmitter<void>();
}
```

**Usage**:

```html
<!-- No tasks empty state -->
<fh-empty-state
  icon="check-circle"
  title="No tasks yet"
  description="Get started by creating your first task for the family."
  actionText="Create Task"
  actionIcon="plus"
  (action)="openCreateTaskModal()"
/>
```

---

**Document Status:** Interaction Design Guide Complete
**Next Steps:** Implement animations, test on devices, refine timings
**Related Documents:** design-system.md, angular-component-specs.md

