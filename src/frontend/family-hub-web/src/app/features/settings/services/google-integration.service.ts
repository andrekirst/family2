import { Injectable, inject, signal, computed } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { map, catchError, of } from 'rxjs';
import {
  GET_LINKED_ACCOUNTS,
  GET_GOOGLE_AUTH_URL,
  UNLINK_GOOGLE_ACCOUNT,
  GET_CALENDAR_SYNC_STATUS,
} from '../graphql/google-integration.operations';
import { LinkedAccount, GoogleCalendarSyncStatus } from '../models/google-integration.models';

@Injectable({ providedIn: 'root' })
export class GoogleIntegrationService {
  private readonly apollo = inject(Apollo);

  readonly linkedAccounts = signal<LinkedAccount[]>([]);
  readonly syncStatus = signal<GoogleCalendarSyncStatus | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly isLinked = computed(() => this.linkedAccounts().length > 0);
  readonly primaryAccount = computed(() => this.linkedAccounts()[0] ?? null);

  loadLinkedAccounts(): void {
    this.loading.set(true);
    this.error.set(null);

    this.apollo
      .query<{ googleIntegration: { linkedAccounts: LinkedAccount[] } }>({
        query: GET_LINKED_ACCOUNTS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data.googleIntegration.linkedAccounts),
        catchError((err) => {
          this.error.set(err.message);
          return of([]);
        }),
      )
      .subscribe((accounts) => {
        this.linkedAccounts.set(accounts);
        this.loading.set(false);
      });
  }

  loadSyncStatus(): void {
    this.apollo
      .query<{ googleIntegration: { calendarSyncStatus: GoogleCalendarSyncStatus } }>({
        query: GET_CALENDAR_SYNC_STATUS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data.googleIntegration.calendarSyncStatus),
        catchError(() => of(null)),
      )
      .subscribe((status) => this.syncStatus.set(status));
  }

  linkGoogle(): void {
    this.loading.set(true);

    this.apollo
      .query<{ googleIntegration: { authUrl: string } }>({
        query: GET_GOOGLE_AUTH_URL,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data.googleIntegration.authUrl),
        catchError((err) => {
          this.error.set(err.message);
          this.loading.set(false);
          return of(null);
        }),
      )
      .subscribe((url) => {
        if (url) {
          window.location.href = url;
        }
      });
  }

  unlinkGoogle(): void {
    this.loading.set(true);
    this.error.set(null);

    this.apollo
      .mutate<{ googleIntegration: { unlink: boolean } }>({
        mutation: UNLINK_GOOGLE_ACCOUNT,
      })
      .pipe(
        map((result) => result.data?.googleIntegration?.unlink ?? false),
        catchError((err) => {
          this.error.set(err.message);
          return of(false);
        }),
      )
      .subscribe((success) => {
        if (success) {
          this.linkedAccounts.set([]);
          this.syncStatus.set(null);
        }
        this.loading.set(false);
      });
  }
}
