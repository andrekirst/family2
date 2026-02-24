import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChainDefinitionService } from '../../services/chain-definition.service';
import { ChainDefinitionDto } from '../../models/chain-definition.models';
import { UserService } from '../../../../core/user/user.service';
import { TopBarService } from '../../../../shared/services/top-bar.service';

@Component({
  selector: 'app-automations-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8 w-full">
      @if (isLoading()) {
        <!-- Loading skeleton -->
        <div class="animate-pulse space-y-4 p-6" data-testid="automations-loading">
          <div class="h-10 bg-gray-200 rounded w-full"></div>
          <div class="h-10 bg-gray-200 rounded w-full"></div>
          <div class="h-10 bg-gray-200 rounded w-full"></div>
        </div>
      } @else if (chainDefinitions().length === 0) {
        <!-- Empty state -->
        <div
          class="flex flex-col items-center justify-center py-20 px-6"
          data-testid="automations-empty"
        >
          <div class="w-16 h-16 rounded-full bg-gray-100 flex items-center justify-center mb-4">
            <svg
              class="h-8 w-8 text-gray-400"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M13 10V3L4 14h7v7l9-11h-7z"
              />
            </svg>
          </div>
          <h3 class="text-lg font-medium text-gray-900 mb-1" i18n="@@automations.empty.title">
            No automations yet
          </h3>
          <p
            class="text-sm text-gray-500 text-center max-w-sm"
            i18n="@@automations.empty.description"
          >
            Event chain automations let you connect events across your family's calendar, health,
            and more.
          </p>
        </div>
      } @else {
        <!-- Data table -->
        <div class="overflow-x-auto" data-testid="automations-table">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                  i18n="@@automations.table.name"
                >
                  Name
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                  i18n="@@automations.table.trigger"
                >
                  Trigger
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                  i18n="@@automations.table.steps"
                >
                  Steps
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                  i18n="@@automations.table.status"
                >
                  Status
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                  i18n="@@automations.table.lastRun"
                >
                  Last Run
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                  i18n="@@automations.table.created"
                >
                  Created
                </th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              @for (chain of chainDefinitions(); track chain.id) {
                <tr class="hover:bg-gray-50 transition-colors">
                  <td class="px-6 py-4">
                    <div class="text-sm font-medium text-gray-900">{{ chain.name }}</div>
                    @if (chain.description) {
                      <div class="text-sm text-gray-500 truncate max-w-xs">
                        {{ chain.description }}
                      </div>
                    }
                  </td>
                  <td class="px-6 py-4 text-sm text-gray-700">
                    {{ chain.trigger.eventType }}
                  </td>
                  <td class="px-6 py-4 text-sm text-gray-700">
                    {{ chain.steps.length }}
                  </td>
                  <td class="px-6 py-4">
                    <span
                      class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                      [class.bg-green-100]="chain.isEnabled"
                      [class.text-green-800]="chain.isEnabled"
                      [class.bg-gray-100]="!chain.isEnabled"
                      [class.text-gray-800]="!chain.isEnabled"
                    >
                      {{ chain.isEnabled ? activeLabel : pausedLabel }}
                    </span>
                  </td>
                  <td class="px-6 py-4 text-sm text-gray-500">
                    {{
                      chain.lastExecutedAt ? (chain.lastExecutedAt | date: 'medium') : neverLabel
                    }}
                  </td>
                  <td class="px-6 py-4 text-sm text-gray-500">
                    {{ chain.createdAt | date: 'mediumDate' }}
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `,
})
export class AutomationsListComponent implements OnInit, OnDestroy {
  private chainDefinitionService = inject(ChainDefinitionService);
  private userService = inject(UserService);
  private topBarService = inject(TopBarService);

  readonly activeLabel = $localize`:@@automations.table.statusActive:Active`;
  readonly pausedLabel = $localize`:@@automations.table.statusPaused:Paused`;
  readonly neverLabel = $localize`:@@automations.table.neverRun:Never`;

  chainDefinitions = signal<ChainDefinitionDto[]>([]);
  isLoading = signal(true);

  ngOnInit(): void {
    this.topBarService.setConfig({ title: $localize`:@@nav.automations:Automations` });
    this.loadChainDefinitions();
  }

  ngOnDestroy(): void {
    this.topBarService.clear();
  }

  private loadChainDefinitions(): void {
    const user = this.userService.currentUser();
    if (!user?.familyId) {
      this.isLoading.set(false);
      return;
    }

    this.chainDefinitionService.getChainDefinitions(user.familyId).subscribe({
      next: (definitions) => {
        this.chainDefinitions.set(definitions);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }
}
