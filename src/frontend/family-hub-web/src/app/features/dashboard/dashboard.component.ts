import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../core/user/user.service';
import { WidgetRegistryService } from '../../core/dashboard/widget-registry.service';
import { WidgetRegistration } from '../../core/dashboard/widget-registry.model';
import { TopBarService } from '../../shared/services/top-bar.service';
import { DashboardService } from './services/dashboard.service';
import { DashboardStateService } from './services/dashboard-state.service';
import { DashboardWidgetDto } from './graphql/dashboard.operations';
import { WidgetContainerComponent } from './components/widget-container/widget-container.component';
import { WidgetPickerComponent } from './components/widget-picker/widget-picker.component';
import { CreateFamilyDialogComponent } from '../family/components/create-family-dialog/create-family-dialog.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    WidgetContainerComponent,
    WidgetPickerComponent,
    CreateFamilyDialogComponent,
  ],
  template: `
    <div class="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8 w-full">
      <!-- Success Alert -->
      @if (showSuccessMessage()) {
        <div class="fixed top-4 right-4 z-50 animate-fade-in">
          <div class="bg-green-50 border-l-4 border-green-500 p-4 shadow-lg rounded">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <svg class="h-5 w-5 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                  <path
                    fill-rule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                    clip-rule="evenodd"
                  />
                </svg>
              </div>
              <div class="ml-3">
                <p class="text-sm font-medium text-green-800">Successfully logged in!</p>
              </div>
            </div>
          </div>
        </div>
      }

      @if (state.isLoading()) {
        <div class="bg-white shadow rounded-lg p-6">
          <div class="animate-pulse">
            <div class="h-6 bg-gray-200 rounded w-1/3 mb-4"></div>
            <div class="h-4 bg-gray-200 rounded w-1/2"></div>
          </div>
        </div>
      } @else {
        <!-- Dashboard Toolbar -->
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-semibold text-gray-900">{{ state.dashboardName() }}</h2>
          <div class="flex items-center gap-2">
            @if (state.isEditMode()) {
              <button
                (click)="addWidget()"
                class="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-blue-600 bg-blue-50 rounded-md hover:bg-blue-100 transition-colors"
              >
                <svg
                  class="w-4 h-4"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke-width="1.5"
                  stroke="currentColor"
                >
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                </svg>
                Add Widget
              </button>
              <button
                (click)="saveDashboard()"
                [disabled]="state.isSaving()"
                class="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50 transition-colors"
              >
                @if (state.isSaving()) {
                  Saving...
                } @else {
                  Save
                }
              </button>
              <button
                (click)="cancelEdit()"
                class="px-3 py-1.5 text-sm font-medium text-gray-600 bg-gray-100 rounded-md hover:bg-gray-200 transition-colors"
              >
                Cancel
              </button>
            } @else {
              <button
                (click)="state.enterEditMode()"
                class="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-gray-600 bg-gray-100 rounded-md hover:bg-gray-200 transition-colors"
              >
                <svg
                  class="w-4 h-4"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke-width="1.5"
                  stroke="currentColor"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.325.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 011.37.49l1.296 2.247a1.125 1.125 0 01-.26 1.431l-1.003.827c-.293.241-.438.613-.43.992a7.723 7.723 0 010 .255c-.008.378.137.75.43.991l1.004.827c.424.35.534.955.26 1.43l-1.298 2.247a1.125 1.125 0 01-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.47 6.47 0 01-.22.128c-.331.183-.581.495-.644.869l-.213 1.281c-.09.543-.56.94-1.11.94h-2.594c-.55 0-1.019-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 01-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 01-1.369-.49l-1.297-2.247a1.125 1.125 0 01.26-1.431l1.004-.827c.292-.24.437-.613.43-.991a6.932 6.932 0 010-.255c.007-.38-.138-.751-.43-.992l-1.004-.827a1.125 1.125 0 01-.26-1.43l1.297-2.247a1.125 1.125 0 011.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.086.22-.128.332-.183.582-.495.644-.869l.214-1.28z"
                  />
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                  />
                </svg>
                Customize
              </button>
            }
          </div>
        </div>

        <!-- Widget Grid -->
        @if (widgets().length > 0) {
          <div class="grid grid-cols-12 gap-4 auto-rows-[minmax(120px,auto)]">
            @for (widget of widgets(); track widget.id) {
              <div
                [style.grid-column]="'span ' + widget.width"
                [style.grid-row]="'span ' + widget.height"
              >
                <app-widget-container
                  [widget]="widget"
                  [isEditMode]="state.isEditMode"
                  (onRemove)="removeWidget(widget.id)"
                />
              </div>
            }
          </div>
        } @else if (!state.isEditMode()) {
          <!-- Empty State -->
          <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-12 text-center">
            <svg
              class="mx-auto h-12 w-12 text-gray-300"
              fill="none"
              viewBox="0 0 24 24"
              stroke-width="1"
              stroke="currentColor"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M3.75 6A2.25 2.25 0 016 3.75h2.25A2.25 2.25 0 0110.5 6v2.25a2.25 2.25 0 01-2.25 2.25H6a2.25 2.25 0 01-2.25-2.25V6zM3.75 15.75A2.25 2.25 0 016 13.5h2.25a2.25 2.25 0 012.25 2.25V18a2.25 2.25 0 01-2.25 2.25H6A2.25 2.25 0 013.75 18v-2.25zM13.5 6a2.25 2.25 0 012.25-2.25H18A2.25 2.25 0 0120.25 6v2.25A2.25 2.25 0 0118 10.5h-2.25a2.25 2.25 0 01-2.25-2.25V6zM13.5 15.75a2.25 2.25 0 012.25-2.25H18a2.25 2.25 0 012.25 2.25V18A2.25 2.25 0 0118 20.25h-2.25A2.25 2.25 0 0113.5 18v-2.25z"
              />
            </svg>
            <h3 class="mt-4 text-sm font-medium text-gray-900">No widgets yet</h3>
            <p class="mt-1 text-sm text-gray-500">
              Click "Customize" to add widgets to your dashboard.
            </p>
            <button
              (click)="state.enterEditMode()"
              class="mt-4 inline-flex items-center gap-1.5 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 transition-colors"
            >
              Get Started
            </button>
          </div>
        }
      }

      <!-- Widget Picker Modal -->
      @if (showWidgetPicker()) {
        <app-widget-picker
          (onClose)="showWidgetPicker.set(false)"
          (onWidgetSelected)="onWidgetSelected($event)"
        />
      }

      <!-- Create Family Dialog -->
      @if (showCreateFamilyDialog()) {
        <app-create-family-dialog
          (familyCreated)="onFamilyCreated()"
          (dialogClosed)="showCreateFamilyDialog.set(false)"
        />
      }
    </div>
  `,
})
export class DashboardComponent implements OnInit {
  private userService = inject(UserService);
  private dashboardService = inject(DashboardService);
  private widgetRegistry = inject(WidgetRegistryService);
  private topBarService = inject(TopBarService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  state = inject(DashboardStateService);

  showSuccessMessage = signal(false);
  showWidgetPicker = signal(false);
  showCreateFamilyDialog = signal(false);

  // Snapshot for cancel support
  private widgetsSnapshot: DashboardWidgetDto[] = [];

  widgets = computed(() => this.state.layout()?.widgets ?? []);

  async ngOnInit(): Promise<void> {
    this.topBarService.setConfig({ title: 'Family Hub' });

    // Fetch user data from backend (skip if already loaded by login callback)
    if (!this.userService.currentUser()) {
      try {
        await this.userService.fetchCurrentUser();
      } catch (error) {
        console.error('Failed to fetch user data:', error);
      }
    }

    // Load the dashboard
    this.loadDashboard();

    // Check for login success query parameter
    this.route.queryParams.subscribe((params) => {
      if (params['login'] === 'success') {
        this.showSuccessMessage.set(true);
        setTimeout(() => {
          this.showSuccessMessage.set(false);
          this.router.navigate([], { queryParams: {}, replaceUrl: true });
        }, 3000);
      }
    });
  }

  private loadDashboard(): void {
    this.state.isLoading.set(true);
    this.dashboardService.getMyDashboard().subscribe({
      next: (layout) => {
        if (layout) {
          this.state.setLayout(layout);
        } else {
          // No dashboard exists yet — create a default layout
          this.createDefaultDashboard();
        }
        this.state.isLoading.set(false);
      },
      error: () => {
        this.state.isLoading.set(false);
      },
    });
  }

  private createDefaultDashboard(): void {
    const defaultWidgets = this.buildDefaultWidgets();
    this.dashboardService
      .saveLayout({
        name: 'My Dashboard',
        isShared: false,
        widgets: defaultWidgets,
      })
      .subscribe({
        next: (layout) => this.state.setLayout(layout),
        error: () => {
          // Failed to create default — show empty state
        },
      });
  }

  private buildDefaultWidgets(): {
    widgetType: string;
    x: number;
    y: number;
    width: number;
    height: number;
    sortOrder: number;
  }[] {
    const allWidgets = this.widgetRegistry.getAll();
    const widgets: {
      widgetType: string;
      x: number;
      y: number;
      width: number;
      height: number;
      sortOrder: number;
    }[] = [];

    let y = 0;
    let sortOrder = 0;
    for (const reg of allWidgets) {
      widgets.push({
        widgetType: reg.id,
        x: 0,
        y,
        width: reg.defaultWidth,
        height: reg.defaultHeight,
        sortOrder: sortOrder++,
      });
      y += reg.defaultHeight;
    }

    return widgets;
  }

  addWidget(): void {
    this.showWidgetPicker.set(true);
  }

  onWidgetSelected(registration: WidgetRegistration): void {
    const layout = this.state.layout();
    if (!layout) return;

    // Find the next available Y position
    const maxY = layout.widgets.reduce((max, w) => Math.max(max, w.y + w.height), 0);

    this.dashboardService
      .addWidget({
        dashboardId: layout.id,
        widgetType: registration.id,
        x: 0,
        y: maxY,
        width: registration.defaultWidth,
        height: registration.defaultHeight,
      })
      .subscribe({
        next: (widget) => {
          this.state.updateWidgets([...layout.widgets, widget]);
        },
      });
  }

  removeWidget(widgetId: string): void {
    this.dashboardService.removeWidget(widgetId).subscribe({
      next: () => {
        this.state.updateWidgets(this.widgets().filter((w) => w.id !== widgetId));
      },
    });
  }

  saveDashboard(): void {
    const layout = this.state.layout();
    if (!layout) return;

    this.state.isSaving.set(true);
    this.dashboardService
      .saveLayout({
        name: layout.name,
        isShared: layout.isShared,
        widgets: layout.widgets.map((w, i) => ({
          id: w.id,
          widgetType: w.widgetType,
          x: w.x,
          y: w.y,
          width: w.width,
          height: w.height,
          sortOrder: i,
          configJson: w.configJson,
        })),
      })
      .subscribe({
        next: (saved) => {
          this.state.setLayout(saved);
          this.state.isSaving.set(false);
          this.state.exitEditMode();
        },
        error: () => this.state.isSaving.set(false),
      });
  }

  cancelEdit(): void {
    // Reload the dashboard from the server to discard unsaved changes
    this.state.exitEditMode();
    this.loadDashboard();
  }

  onFamilyCreated(): void {
    this.showCreateFamilyDialog.set(false);
    this.userService.fetchCurrentUser();
  }
}
