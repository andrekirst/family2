import {
  Component,
  Input,
  Output,
  EventEmitter,
  Signal,
  signal,
  computed,
  inject,
  ViewContainerRef,
  OnInit,
  OnDestroy,
  ComponentRef,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { WidgetRegistryService } from '../../../../core/dashboard/widget-registry.service';
import { DashboardWidgetComponent } from '../../../../core/dashboard/dashboard-widget.interface';
import { DashboardWidgetDto } from '../../graphql/dashboard.operations';

@Component({
  selector: 'app-widget-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="bg-white rounded-lg shadow-sm border border-gray-200 h-full flex flex-col overflow-hidden"
      [class.ring-2]="isEditMode()"
      [class.ring-blue-300]="isEditMode()"
    >
      <!-- Widget Header -->
      <div
        class="flex items-center justify-between px-3 py-2 border-b border-gray-100 bg-gray-50/50"
      >
        <h3 class="text-sm font-medium text-gray-700 truncate">
          {{ widgetTitle() }}
        </h3>
        @if (isEditMode()) {
          <div class="flex items-center gap-1">
            <button
              (click)="onRemove.emit()"
              class="p-1 text-gray-400 hover:text-red-500 rounded transition-colors"
              title="Remove widget"
            >
              <svg
                class="w-4 h-4"
                fill="none"
                viewBox="0 0 24 24"
                stroke-width="1.5"
                stroke="currentColor"
              >
                <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        }
      </div>

      <!-- Widget Body -->
      <div class="flex-1 p-3 overflow-auto">
        <ng-template #widgetHost></ng-template>
      </div>
    </div>
  `,
})
export class WidgetContainerComponent implements OnInit, OnDestroy {
  @Input({ required: true }) widget!: DashboardWidgetDto;
  @Input() isEditMode: Signal<boolean> = signal(false);
  @Output() onRemove = new EventEmitter<void>();

  @ViewChild('widgetHost', { read: ViewContainerRef, static: true })
  widgetHost!: ViewContainerRef;

  private registryService = inject(WidgetRegistryService);
  private componentRef: ComponentRef<DashboardWidgetComponent> | null = null;

  widgetTitle = computed(() => {
    const reg = this.registryService.getById(this.widget?.widgetType);
    return reg?.title ?? this.widget?.widgetType ?? 'Widget';
  });

  ngOnInit(): void {
    this.loadWidget();
  }

  ngOnDestroy(): void {
    this.componentRef?.destroy();
  }

  private loadWidget(): void {
    const registration = this.registryService.getById(this.widget.widgetType);
    if (!registration) return;

    this.widgetHost.clear();
    this.componentRef = this.widgetHost.createComponent(registration.component);

    // Pass config to the widget
    if (this.widget.configJson) {
      try {
        const config = JSON.parse(this.widget.configJson);
        this.componentRef.instance.widgetConfig.set(config);
      } catch {
        // Invalid JSON config, ignore
      }
    }
  }
}
