import { Component, OnDestroy, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HealthService, HealthStatus } from '../../shared/services/health.service';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-status',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-gray-50 flex flex-col items-center justify-center p-6">
      <div class="max-w-lg w-full">
        <!-- Overall status banner -->
        <div
          class="rounded-lg p-6 mb-6 text-center"
          [class.bg-green-50]="overallHealthy()"
          [class.border-green-200]="overallHealthy()"
          [class.bg-red-50]="!overallHealthy()"
          [class.border-red-200]="!overallHealthy()"
          [class.border]="true"
        >
          <div class="text-4xl mb-2">{{ overallHealthy() ? '&#9989;' : '&#10060;' }}</div>
          <h1
            class="text-2xl font-bold"
            [class.text-green-800]="overallHealthy()"
            [class.text-red-800]="!overallHealthy()"
          >
            {{ overallHealthy() ? 'All Systems Operational' : 'System Issues Detected' }}
          </h1>
          <p class="text-sm text-gray-500 mt-2">
            Status: {{ healthStatus()?.status ?? 'Loading...' }}
          </p>
        </div>

        <!-- Individual checks -->
        @for (check of checkEntries(); track check.name) {
          <div class="bg-white rounded-lg border p-4 mb-3 flex items-center justify-between">
            <div>
              <h3 class="font-medium text-gray-900">{{ formatCheckName(check.name) }}</h3>
              <p class="text-sm text-gray-500">{{ check.detail.description }}</p>
            </div>
            <span
              class="inline-flex items-center rounded-full px-3 py-1 text-sm font-medium"
              [class.bg-green-100]="check.detail.status === 'Healthy'"
              [class.text-green-800]="check.detail.status === 'Healthy'"
              [class.bg-red-100]="check.detail.status !== 'Healthy'"
              [class.text-red-800]="check.detail.status !== 'Healthy'"
            >
              {{ check.detail.status }}
            </span>
          </div>
        }

        <!-- Last checked timestamp -->
        <p class="text-center text-sm text-gray-400 mt-4">Last checked: {{ lastCheckedAgo() }}</p>

        <!-- Actions -->
        <div class="mt-6 flex justify-center gap-4">
          <button
            (click)="goToLogin()"
            class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Return to Login
          </button>
          <button
            (click)="refreshNow()"
            class="px-6 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition-colors"
          >
            Refresh Now
          </button>
        </div>
      </div>
    </div>
  `,
})
export class StatusComponent implements OnInit, OnDestroy {
  private healthService = inject(HealthService);
  private authService = inject(AuthService);
  private router = inject(Router);

  healthStatus = signal<HealthStatus | null>(null);
  lastChecked = signal<Date | null>(null);

  private intervalId: ReturnType<typeof setInterval> | null = null;

  overallHealthy = computed(() => this.healthStatus()?.status === 'Healthy');

  checkEntries = computed(() => {
    const status = this.healthStatus();
    if (!status?.checks) return [];
    return Object.entries(status.checks).map(([name, detail]) => ({ name, detail }));
  });

  lastCheckedAgo = computed(() => {
    const checked = this.lastChecked();
    if (!checked) return 'Never';
    const seconds = Math.floor((Date.now() - checked.getTime()) / 1000);
    if (seconds < 5) return 'Just now';
    return `${seconds}s ago`;
  });

  ngOnInit(): void {
    this.fetchHealth();
    this.intervalId = setInterval(() => this.fetchHealth(), 5000);
  }

  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  goToLogin(): void {
    this.authService.login();
  }

  refreshNow(): void {
    this.fetchHealth();
  }

  formatCheckName(name: string): string {
    return name
      .split('_')
      .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
  }

  private fetchHealth(): void {
    this.healthService.checkHealth().subscribe((status) => {
      this.healthStatus.set(status);
      this.lastChecked.set(new Date());
    });
  }
}
