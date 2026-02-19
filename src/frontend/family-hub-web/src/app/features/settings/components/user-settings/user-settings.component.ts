import { Component, inject, effect, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { TopBarService } from '../../../../shared/services/top-bar.service';
import { IntegrationsPanelComponent } from '../integrations-panel/integrations-panel.component';
import { GoogleIntegrationService } from '../../services/google-integration.service';

@Component({
  selector: 'app-user-settings',
  standalone: true,
  imports: [CommonModule, IntegrationsPanelComponent],
  template: `
    @if (successMessage) {
      <div class="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg">
        <p class="text-sm text-green-800">{{ successMessage }}</p>
      </div>
    }

    @if (errorMessage) {
      <div class="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
        <p class="text-sm text-red-800">{{ errorMessage }}</p>
      </div>
    }

    <div class="max-w-2xl space-y-8">
      <app-integrations-panel />
    </div>
  `,
})
export class UserSettingsComponent implements OnDestroy {
  private readonly topBarService = inject(TopBarService);
  private readonly route = inject(ActivatedRoute);
  private readonly googleService = inject(GoogleIntegrationService);

  successMessage: string | null = null;
  errorMessage: string | null = null;

  private readonly topBarEffect = effect(() => {
    this.topBarService.setConfig({
      title: 'Settings',
    });
  });

  constructor() {
    // Check for OAuth callback query params
    this.route.queryParams.subscribe((params) => {
      if (params['google_linked'] === 'true') {
        this.successMessage = 'Google account linked successfully!';
        this.googleService.loadLinkedAccounts();
      }
      if (params['google_error']) {
        this.errorMessage = `Failed to link Google account: ${params['google_error']}`;
      }
    });
  }

  ngOnDestroy(): void {
    this.topBarService.clear();
  }
}
