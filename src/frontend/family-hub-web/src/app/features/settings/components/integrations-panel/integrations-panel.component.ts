import { Component } from '@angular/core';
import { GoogleLinkComponent } from '../google-link/google-link.component';

@Component({
  selector: 'app-integrations-panel',
  standalone: true,
  imports: [GoogleLinkComponent],
  template: `
    <div class="space-y-4">
      <div>
        <h2 class="text-lg font-semibold text-gray-900">Integrations</h2>
        <p class="text-sm text-gray-500">
          Connect external accounts to enhance your Family Hub experience.
        </p>
      </div>
      <app-google-link />
    </div>
  `,
})
export class IntegrationsPanelComponent {}
