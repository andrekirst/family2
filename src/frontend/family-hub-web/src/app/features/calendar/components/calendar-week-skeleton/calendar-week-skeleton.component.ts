import { Component } from '@angular/core';

@Component({
  selector: 'app-calendar-week-skeleton',
  standalone: true,
  template: `
    <div class="bg-white shadow rounded-lg overflow-hidden" data-testid="week-skeleton">
      <!-- Day header placeholders -->
      <div class="grid grid-cols-[4rem_repeat(7,1fr)] border-b border-gray-200">
        <div class="w-16"></div>
        @for (i of headerPlaceholders; track i) {
          <div class="py-3 px-2 text-center border-l border-gray-200">
            <div class="h-4 w-10 bg-gray-200 rounded animate-pulse mx-auto mb-1"></div>
            <div class="h-6 w-6 bg-gray-200 rounded-full animate-pulse mx-auto"></div>
          </div>
        }
      </div>

      <!-- All-day row placeholder -->
      <div class="grid grid-cols-[4rem_repeat(7,1fr)] border-b border-gray-200">
        <div class="w-16 py-2 px-1">
          <div class="h-3 w-12 bg-gray-200 rounded animate-pulse"></div>
        </div>
        @for (i of headerPlaceholders; track i) {
          <div class="py-2 px-1 border-l border-gray-200">
            <div class="h-5 bg-gray-100 rounded animate-pulse"></div>
          </div>
        }
      </div>

      <!-- Time grid placeholders -->
      <div class="overflow-hidden" style="height: 480px">
        @for (hour of hourPlaceholders; track hour) {
          <div class="grid grid-cols-[4rem_repeat(7,1fr)] h-[60px] border-b border-gray-100">
            <div class="w-16 px-1 pt-1">
              <div class="h-3 w-10 bg-gray-200 rounded animate-pulse"></div>
            </div>
            @for (day of headerPlaceholders; track day) {
              <div class="border-l border-gray-100"></div>
            }
          </div>
        }
      </div>
    </div>
  `,
})
export class CalendarWeekSkeletonComponent {
  readonly headerPlaceholders = [1, 2, 3, 4, 5, 6, 7];
  readonly hourPlaceholders = [1, 2, 3, 4, 5, 6, 7, 8];
}
