import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CalendarViewMode } from '../../models/calendar.models';

@Component({
  selector: 'app-calendar-view-switcher',
  standalone: true,
  template: `
    <div
      class="inline-flex rounded-lg border border-gray-300 overflow-hidden"
      data-testid="view-switcher"
    >
      <button
        type="button"
        (click)="viewChanged.emit('month')"
        class="px-4 py-2 text-sm font-medium transition-colors"
        [class.bg-blue-600]="activeView === 'month'"
        [class.text-white]="activeView === 'month'"
        [class.bg-white]="activeView !== 'month'"
        [class.text-gray-700]="activeView !== 'month'"
        [class.hover:bg-gray-50]="activeView !== 'month'"
        data-testid="view-month-btn"
      >
        <span i18n="@@calendar.viewMonth">Month</span>
      </button>
      <button
        type="button"
        (click)="viewChanged.emit('week')"
        class="px-4 py-2 text-sm font-medium transition-colors border-l border-gray-300"
        [class.bg-blue-600]="activeView === 'week'"
        [class.text-white]="activeView === 'week'"
        [class.bg-white]="activeView !== 'week'"
        [class.text-gray-700]="activeView !== 'week'"
        [class.hover:bg-gray-50]="activeView !== 'week'"
        data-testid="view-week-btn"
      >
        <span i18n="@@calendar.viewWeek">Week</span>
      </button>
      <button
        type="button"
        (click)="viewChanged.emit('day')"
        class="px-4 py-2 text-sm font-medium transition-colors border-l border-gray-300"
        [class.bg-blue-600]="activeView === 'day'"
        [class.text-white]="activeView === 'day'"
        [class.bg-white]="activeView !== 'day'"
        [class.text-gray-700]="activeView !== 'day'"
        [class.hover:bg-gray-50]="activeView !== 'day'"
        data-testid="view-day-btn"
      >
        <span i18n="@@calendar.viewDay">Day</span>
      </button>
    </div>
  `,
})
export class CalendarViewSwitcherComponent {
  @Input() activeView: CalendarViewMode = 'month';
  @Output() viewChanged = new EventEmitter<CalendarViewMode>();
}
