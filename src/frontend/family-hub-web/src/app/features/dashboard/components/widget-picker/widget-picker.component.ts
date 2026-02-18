import { Component, Output, EventEmitter, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WidgetRegistryService } from '../../../../core/dashboard/widget-registry.service';
import { WidgetRegistration } from '../../../../core/dashboard/widget-registry.model';

@Component({
  selector: 'app-widget-picker',
  standalone: true,
  imports: [CommonModule],
  template: `
    <!-- Backdrop -->
    <div class="fixed inset-0 z-40 bg-black/30 backdrop-blur-sm" (click)="onClose.emit()"></div>

    <!-- Modal -->
    <div class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div
        class="bg-white rounded-xl shadow-xl max-w-lg w-full max-h-[80vh] flex flex-col"
        (click)="$event.stopPropagation()"
      >
        <!-- Header -->
        <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200">
          <h2 class="text-lg font-semibold text-gray-900">Add Widget</h2>
          <button
            (click)="onClose.emit()"
            class="p-1 text-gray-400 hover:text-gray-600 rounded-md transition-colors"
          >
            <svg
              class="w-5 h-5"
              fill="none"
              viewBox="0 0 24 24"
              stroke-width="1.5"
              stroke="currentColor"
            >
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Widget List -->
        <div class="flex-1 overflow-y-auto p-4">
          @if (availableWidgets().length === 0) {
            <div class="text-center py-8">
              <p class="text-sm text-gray-500">No widgets available.</p>
            </div>
          } @else {
            <div class="space-y-2">
              @for (widget of availableWidgets(); track widget.id) {
                <button
                  (click)="selectWidget(widget)"
                  class="w-full text-left p-4 rounded-lg border border-gray-200 hover:border-blue-300 hover:bg-blue-50/50 transition-all group"
                >
                  <div class="flex items-start justify-between">
                    <div>
                      <p class="text-sm font-medium text-gray-900 group-hover:text-blue-700">
                        {{ widget.title }}
                      </p>
                      <p class="text-xs text-gray-500 mt-0.5">{{ widget.description }}</p>
                      <p class="text-xs text-gray-400 mt-1">
                        Size: {{ widget.defaultWidth }}x{{ widget.defaultHeight }}
                      </p>
                    </div>
                    <svg
                      class="w-5 h-5 text-gray-300 group-hover:text-blue-500 flex-shrink-0 mt-0.5"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke-width="1.5"
                      stroke="currentColor"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        d="M12 4.5v15m7.5-7.5h-15"
                      />
                    </svg>
                  </div>
                </button>
              }
            </div>
          }
        </div>
      </div>
    </div>
  `,
})
export class WidgetPickerComponent implements OnInit {
  @Output() onClose = new EventEmitter<void>();
  @Output() onWidgetSelected = new EventEmitter<WidgetRegistration>();

  private registryService = inject(WidgetRegistryService);
  availableWidgets = signal<WidgetRegistration[]>([]);

  ngOnInit(): void {
    this.availableWidgets.set(this.registryService.getAll());
  }

  selectWidget(widget: WidgetRegistration): void {
    this.onWidgetSelected.emit(widget);
    this.onClose.emit();
  }
}
