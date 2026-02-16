import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardWidgetComponent } from '../../../../core/dashboard/dashboard-widget.interface';

@Component({
  selector: 'app-upcoming-events-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="text-center py-4">
      <svg
        class="mx-auto h-8 w-8 text-gray-300"
        fill="none"
        viewBox="0 0 24 24"
        stroke-width="1.5"
        stroke="currentColor"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          d="M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 012.25-2.25h13.5A2.25 2.25 0 0121 7.5v11.25m-18 0A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75m-18 0v-7.5A2.25 2.25 0 015.25 9h13.5A2.25 2.25 0 0121 11.25v7.5"
        />
      </svg>
      <p class="mt-2 text-sm text-gray-500">No upcoming events.</p>
      <p class="text-xs text-gray-400">Events will appear here once your calendar has entries.</p>
    </div>
  `,
})
export class UpcomingEventsWidgetComponent implements DashboardWidgetComponent {
  widgetConfig = signal<Record<string, unknown> | null>(null);
}
